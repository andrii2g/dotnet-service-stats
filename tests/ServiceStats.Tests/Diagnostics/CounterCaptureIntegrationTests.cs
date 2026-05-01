using A2G.ServiceStats.Diagnostics;
using A2G.ServiceStats.Models;
using A2G.ServiceStats.TestTarget;
using System.Diagnostics;

namespace A2G.ServiceStats.Tests.Diagnostics;

public sealed class CounterCaptureIntegrationTests
{
    [Fact]
    public async Task CaptureAsync_ReturnsSnapshotForManagedTarget()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var targetDll = typeof(Marker).Assembly.Location;
        var startInfo = new ProcessStartInfo("dotnet", $"\"{targetDll}\" --mode alloc")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);

        try
        {
            var line = await process!.StandardOutput.ReadLineAsync(cancellationToken);
            Assert.NotNull(line);

            var pid = int.Parse(line!);
            var captureService = new CounterCaptureService(new EventCounterParser());
            var identity = new ProcessIdentity(pid, "dotnet", null, null, true, true, null);
            var result = await captureService.CaptureAsync(identity, TimeSpan.FromSeconds(3), cancellationToken);

            Assert.Equal(ExitCodes.Success, result.ExitCode);
            Assert.True(result.Snapshot.Process.Pid > 0);
            Assert.True(result.Snapshot.Status is SnapshotStatus.Complete or SnapshotStatus.Partial);
        }
        catch (DiagnosticsException ex) when (ex.ExitCode is ExitCodes.TargetNotAttachable or ExitCodes.PermissionDenied)
        {
            return;
        }
        finally
        {
            if (process is { HasExited: false })
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(cancellationToken);
            }
        }
    }
}
