using System.Collections;
using A2G.ServiceStats.Models;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using System.Diagnostics;

namespace A2G.ServiceStats.Diagnostics;

internal sealed class CounterCaptureService
{
    private readonly EventCounterParser _eventCounterParser;

    public CounterCaptureService(EventCounterParser eventCounterParser)
    {
        _eventCounterParser = eventCounterParser;
    }

    public async Task<CaptureResult> CaptureAsync(
        ProcessIdentity identity,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        try
        {
            using var process = Process.GetProcessById(identity.Pid);
            var warnings = new List<string>();
            var startWallClock = DateTimeOffset.UtcNow;
            var startCpu = SafeGetCpu(process);
            var runtimeAggregator = new RuntimeCounterAggregator();

            using var session = new DiagnosticsClient(identity.Pid).StartEventPipeSession(
                providers: [RuntimeCounterProvider.Create()],
                requestRundown: false);
            using var source = new EventPipeEventSource(session.EventStream);

            source.Dynamic.All += traceEvent =>
            {
                object? payload = null;
                try
                {
                    var payloadValue = traceEvent.PayloadValue(0);
                    if (payloadValue is IDictionary payloadDictionary && payloadDictionary.Contains("Payload"))
                    {
                        payload = payloadDictionary["Payload"];
                    }
                    else
                    {
                        payload = payloadValue;
                    }
                }
                catch
                {
                    payload = null;
                }

                if (_eventCounterParser.TryParse(payload, out var sample) && sample is not null)
                {
                    runtimeAggregator.Observe(sample);
                }
            };

            var processingTask = Task.Run(() =>
            {
                try
                {
                    source.Process();
                }
                catch (Exception ex) when (ex is EndOfStreamException or OperationCanceledException)
                {
                }
            }, CancellationToken.None);

            try
            {
                await Task.Delay(duration, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                warnings.Add("Capture was cancelled before the full duration elapsed.");
            }
            finally
            {
                try
                {
                    session.Stop();
                }
                catch (Exception ex)
                {
                    warnings.Add($"Stopping the EventPipe session reported: {ex.Message}");
                }
            }

            await processingTask;

            var processSnapshot = ProcessMetricsCollector.Capture(process, startCpu, startWallClock, warnings);
            var runtimeSnapshot = runtimeAggregator.Build();
            var status = DetermineStatus(processSnapshot, runtimeSnapshot);

            if (runtimeSnapshot is null)
            {
                warnings.Add("System.Runtime counters were not observed during the capture window.");
            }

            var snapshot = new ServiceStatsSnapshot(
                SchemaVersion: "1.0",
                CapturedAtUtc: DateTimeOffset.UtcNow,
                CaptureDuration: duration,
                Status: status,
                Process: processSnapshot,
                Runtime: runtimeSnapshot,
                Warnings: warnings);

            return new CaptureResult(snapshot, ExitCodes.Success);
        }
        catch (ServerNotAvailableException ex)
        {
            throw new DiagnosticsException(ExitCodes.TargetNotAttachable, $"PID {identity.Pid} is not a published .NET diagnostics process or cannot be attached.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new DiagnosticsException(ExitCodes.PermissionDenied, $"Permission denied while attaching to PID {identity.Pid}. Try running dss with appropriate privileges.", ex);
        }
        catch (OperationCanceledException ex)
        {
            throw new DiagnosticsException(ExitCodes.Timeout, "The capture timed out before a useful snapshot could be produced.", ex);
        }
        catch (Exception ex) when (IsPermissionError(ex))
        {
            throw new DiagnosticsException(ExitCodes.PermissionDenied, $"Permission denied while attaching to PID {identity.Pid}. Try running dss with appropriate privileges.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new DiagnosticsException(ExitCodes.CollectionFailed, $"Failed to collect runtime counters from PID {identity.Pid}.", ex);
        }
    }

    private static bool IsPermissionError(Exception ex)
        => ex.Message.Contains("Access is denied", StringComparison.OrdinalIgnoreCase) ||
           ex.Message.Contains("permission", StringComparison.OrdinalIgnoreCase);

    private static TimeSpan SafeGetCpu(Process process)
    {
        try
        {
            return process.TotalProcessorTime;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }

    private static SnapshotStatus DetermineStatus(ProcessSnapshot processSnapshot, RuntimeSnapshot? runtimeSnapshot)
    {
        if (runtimeSnapshot is null)
        {
            return SnapshotStatus.Partial;
        }

        var hasRuntimeData =
            runtimeSnapshot.GcHeapSizeBytes is not null ||
            runtimeSnapshot.LohSizeBytes is not null ||
            runtimeSnapshot.AllocationRateBytesPerSecond is not null ||
            runtimeSnapshot.GcPauseTimePercentage is not null ||
            runtimeSnapshot.HeapFragmentationPercentage is not null ||
            runtimeSnapshot.ExceptionRatePerSecond is not null ||
            runtimeSnapshot.Gen0CollectionsPerSecond is not null ||
            runtimeSnapshot.Gen1CollectionsPerSecond is not null ||
            runtimeSnapshot.Gen2CollectionsPerSecond is not null ||
            runtimeSnapshot.FinalizationQueueLength is not null ||
            runtimeSnapshot.ActiveTimerCount is not null ||
            runtimeSnapshot.MethodsJittedCount is not null ||
            runtimeSnapshot.IlBytesJitted is not null ||
            runtimeSnapshot.ThreadPoolThreadCount is not null ||
            runtimeSnapshot.ThreadPoolQueueLength is not null ||
            runtimeSnapshot.MonitorLockContentionCountPerSecond is not null;

        if (!hasRuntimeData)
        {
            return SnapshotStatus.Partial;
        }

        var hasMissingProcessMetric =
            processSnapshot.CpuPercent is null ||
            processSnapshot.WorkingSetBytes is null ||
            processSnapshot.PrivateMemoryBytes is null ||
            processSnapshot.VirtualMemoryBytes is null ||
            processSnapshot.ThreadCount is null;

        return hasMissingProcessMetric ? SnapshotStatus.Partial : SnapshotStatus.Complete;
    }

    internal sealed record CaptureResult(ServiceStatsSnapshot Snapshot, int ExitCode);
}
