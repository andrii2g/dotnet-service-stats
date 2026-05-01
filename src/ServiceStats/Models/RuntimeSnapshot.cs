namespace A2G.ServiceStats.Models;

public sealed record RuntimeSnapshot(
    string? RuntimeProviderName,
    long? GcHeapSizeBytes,
    long? LohSizeBytes,
    double? AllocationRateBytesPerSecond,
    double? GcPauseTimePercentage,
    double? HeapFragmentationPercentage,
    double? ExceptionRatePerSecond,
    double? Gen0CollectionsPerSecond,
    double? Gen1CollectionsPerSecond,
    double? Gen2CollectionsPerSecond,
    long? FinalizationQueueLength,
    int? ActiveTimerCount,
    int? MethodsJittedCount,
    long? IlBytesJitted,
    int? ThreadPoolThreadCount,
    int? ThreadPoolQueueLength,
    double? MonitorLockContentionCountPerSecond,
    IReadOnlyDictionary<string, double> RawCounters);
