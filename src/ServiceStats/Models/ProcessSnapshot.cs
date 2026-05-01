namespace A2G.ServiceStats.Models;

public sealed record ProcessSnapshot(
    int Pid,
    string ProcessName,
    DateTimeOffset? StartTimeUtc,
    TimeSpan? Uptime,
    double? CpuPercent,
    long? WorkingSetBytes,
    long? PrivateMemoryBytes,
    long? VirtualMemoryBytes,
    int? HandleCount,
    int? ThreadCount,
    int? LoadedAssemblyCount);
