namespace A2G.ServiceStats.Models;

public sealed record RuntimeSnapshot(
    string? RuntimeProviderName,
    long? GcHeapSizeBytes,
    double? AllocationRateBytesPerSecond,
    double? ExceptionRatePerSecond,
    double? Gen0CollectionsPerSecond,
    double? Gen1CollectionsPerSecond,
    double? Gen2CollectionsPerSecond,
    int? ThreadPoolThreadCount,
    int? ThreadPoolQueueLength,
    double? MonitorLockContentionCountPerSecond,
    IReadOnlyDictionary<string, double> RawCounters);
