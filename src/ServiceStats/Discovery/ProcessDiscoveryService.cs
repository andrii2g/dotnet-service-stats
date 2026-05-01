using A2G.ServiceStats.Diagnostics;
using A2G.ServiceStats.Models;
using System.Diagnostics;

namespace A2G.ServiceStats.Discovery;

internal sealed class ProcessDiscoveryService
{
    private readonly AttachabilityChecker _attachabilityChecker;
    private readonly WindowsServiceResolver _windowsServiceResolver;

    public ProcessDiscoveryService(AttachabilityChecker attachabilityChecker, WindowsServiceResolver windowsServiceResolver)
    {
        _attachabilityChecker = attachabilityChecker;
        _windowsServiceResolver = windowsServiceResolver;
    }

    public Task<ProcessListResult> ListAsync(bool includeAll, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var publishedProcesses = _attachabilityChecker.GetPublishedProcessIds();
        var processes = Process.GetProcesses()
            .OrderBy(p => p.ProcessName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(p => p.Id)
            .ToArray();

        var items = new List<ProcessIdentity>();
        var skipped = 0;

        foreach (var process in processes)
        {
            using (process)
            {
                try
                {
                    var isPublished = publishedProcesses.Contains(process.Id);
                    if (!includeAll && !isPublished)
                    {
                        continue;
                    }

                    items.Add(ToIdentity(process, isPublished));
                }
                catch
                {
                    skipped++;
                }
            }
        }

        return Task.FromResult(new ProcessListResult(items, skipped));
    }

    public Task<ProcessIdentity> ResolveByPidAsync(int pid, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var process = Process.GetProcessById(pid);
            var publishedProcesses = _attachabilityChecker.GetPublishedProcessIds();
            var isPublished = publishedProcesses.Contains(pid);
            var identity = ToIdentity(process, isPublished);

            if (!identity.IsAttachableCandidate)
            {
                throw new DiagnosticsException(ExitCodes.TargetNotAttachable, $"PID {pid} is not a published .NET diagnostics process or cannot be attached.");
            }

            return Task.FromResult(identity);
        }
        catch (ArgumentException ex)
        {
            throw new DiagnosticsException(ExitCodes.TargetNotFound, $"Process with PID {pid} was not found.", ex);
        }
    }

    public async Task<ProcessIdentity> ResolveByNameAsync(string name, CancellationToken cancellationToken)
    {
        var list = await ListAsync(includeAll: true, cancellationToken);
        var matches = list.Processes
            .Where(process => ProcessNameMatcher.Matches(name, process.ProcessName))
            .ToArray();

        if (matches.Length == 0)
        {
            throw new DiagnosticsException(ExitCodes.TargetNotFound, $"No process matched '{name}'.");
        }

        if (matches.Length > 1)
        {
            var candidates = string.Join(", ", matches.Select(match => $"{match.ProcessName} ({match.Pid})"));
            throw new DiagnosticsException(ExitCodes.UsageError, $"Multiple processes matched '{name}': {candidates}");
        }

        if (!matches[0].IsAttachableCandidate)
        {
            throw new DiagnosticsException(ExitCodes.TargetNotAttachable, $"PID {matches[0].Pid} is not a published .NET diagnostics process or cannot be attached.");
        }

        return matches[0];
    }

    public async Task<ProcessIdentity> ResolveByServiceAsync(string serviceName, CancellationToken cancellationToken)
    {
        var pid = await _windowsServiceResolver.ResolveProcessIdAsync(serviceName, cancellationToken);
        return await ResolveByPidAsync(pid, cancellationToken);
    }

    private static ProcessIdentity ToIdentity(Process process, bool isPublished)
    {
        var startTime = SafeGet(() => new DateTimeOffset(process.StartTime.ToUniversalTime()));
        TimeSpan? uptime = startTime is DateTimeOffset startedAt ? DateTimeOffset.UtcNow - startedAt : null;

        return new ProcessIdentity(
            Pid: process.Id,
            ProcessName: process.ProcessName,
            StartTime: startTime,
            Uptime: uptime,
            IsPublishedDotNetProcess: isPublished,
            IsAttachableCandidate: isPublished,
            AttachabilityReason: isPublished ? null : "Process is not currently published through the .NET diagnostics transport.");
    }

    private static T? SafeGet<T>(Func<T> accessor) where T : struct
    {
        try
        {
            return accessor();
        }
        catch
        {
            return null;
        }
    }
}

internal sealed record ProcessListResult(IReadOnlyList<ProcessIdentity> Processes, int SkippedProcessCount);
