using A2G.ServiceStats.Models;
using System.Diagnostics;

namespace A2G.ServiceStats.Diagnostics;

internal static class ProcessMetricsCollector
{
    public static double? ComputeCpuPercent(TimeSpan deltaProcessCpu, TimeSpan deltaWallClock, int processorCount)
    {
        if (deltaWallClock <= TimeSpan.Zero || processorCount <= 0)
        {
            return null;
        }

        var cpuPercent = deltaProcessCpu.TotalMilliseconds / (deltaWallClock.TotalMilliseconds * processorCount) * 100d;
        if (double.IsNaN(cpuPercent) || double.IsInfinity(cpuPercent))
        {
            return null;
        }

        return Math.Clamp(cpuPercent, 0d, 100d);
    }

    public static ProcessSnapshot Capture(
        Process process,
        TimeSpan startCpu,
        DateTimeOffset startWallClock,
        IList<string> warnings)
    {
        process.Refresh();

        var endWallClock = DateTimeOffset.UtcNow;
        TimeSpan endCpu;

        try
        {
            endCpu = process.TotalProcessorTime;
        }
        catch
        {
            endCpu = startCpu;
            warnings.Add("CPU usage could not be read for the target process.");
        }

        return new ProcessSnapshot(
            Pid: process.Id,
            ProcessName: process.ProcessName,
            StartTimeUtc: SafeGetStartTimeUtc(process),
            Uptime: SafeGetStartTimeUtc(process) is DateTimeOffset startTime ? endWallClock - startTime : null,
            CpuPercent: ComputeCpuPercent(endCpu - startCpu, endWallClock - startWallClock, Environment.ProcessorCount),
            WorkingSetBytes: SafeRead(() => process.WorkingSet64),
            PrivateMemoryBytes: SafeRead(() => process.PrivateMemorySize64),
            VirtualMemoryBytes: SafeRead(() => process.VirtualMemorySize64),
            HandleCount: OperatingSystem.IsWindows() ? SafeRead(() => process.HandleCount) : null,
            ThreadCount: SafeRead(() => process.Threads.Count),
            LoadedAssemblyCount: null);
    }

    private static T? SafeRead<T>(Func<T> accessor) where T : struct
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

    private static DateTimeOffset? SafeGetStartTimeUtc(Process process)
    {
        try
        {
            return process.StartTime.ToUniversalTime();
        }
        catch
        {
            return null;
        }
    }
}
