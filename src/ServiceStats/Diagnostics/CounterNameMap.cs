namespace A2G.ServiceStats.Diagnostics;

internal static class CounterNameMap
{
    public const string GcHeapSize = "gc-heap-size";
    public const string LohSize = "loh-size";
    public const string AllocationRate = "alloc-rate";
    public const string TimeInGc = "time-in-gc";
    public const string GcFragmentation = "gc-fragmentation";
    public const string ExceptionCount = "exception-count";
    public const string Gen0Count = "gen-0-gc-count";
    public const string Gen1Count = "gen-1-gc-count";
    public const string Gen2Count = "gen-2-gc-count";
    public const string ActiveTimerCount = "active-timer-count";
    public const string MethodsJittedCount = "methods-jitted-count";
    public const string IlBytesJitted = "il-bytes-jitted";
    public const string ThreadPoolThreadCount = "threadpool-thread-count";
    public const string ThreadPoolQueueLength = "threadpool-queue-length";
    public const string MonitorLockContentionCount = "monitor-lock-contention-count";

    public static readonly IReadOnlySet<string> RequiredCounters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        GcHeapSize,
        LohSize,
        AllocationRate,
        TimeInGc,
        GcFragmentation,
        ExceptionCount,
        Gen0Count,
        Gen1Count,
        Gen2Count,
        ActiveTimerCount,
        MethodsJittedCount,
        IlBytesJitted,
        ThreadPoolThreadCount,
        ThreadPoolQueueLength,
        MonitorLockContentionCount
    };
}
