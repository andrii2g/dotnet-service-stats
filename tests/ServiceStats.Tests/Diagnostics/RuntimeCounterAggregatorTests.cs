using A2G.ServiceStats.Diagnostics;

namespace A2G.ServiceStats.Tests.Diagnostics;

public sealed class RuntimeCounterAggregatorTests
{
    [Fact]
    public void Build_MapsRuntimeDeepMetrics()
    {
        var aggregator = new RuntimeCounterAggregator();

        aggregator.Observe(new CounterSample(CounterNameMap.LohSize, "LOH Size", "Mean", 262_144d, null, new Dictionary<string, object?>()));
        aggregator.Observe(new CounterSample(CounterNameMap.TimeInGc, "% Time in GC since last GC", "Mean", 12.5d, null, new Dictionary<string, object?>()));
        aggregator.Observe(new CounterSample(CounterNameMap.GcFragmentation, "GC Fragmentation", "Mean", 48.5d, null, new Dictionary<string, object?>()));
        aggregator.Observe(new CounterSample(CounterNameMap.ActiveTimerCount, "Number of Active Timers", "Mean", 7d, null, new Dictionary<string, object?>()));
        aggregator.Observe(new CounterSample(CounterNameMap.MethodsJittedCount, "Methods Jitted Count", "Mean", 321d, null, new Dictionary<string, object?>()));
        aggregator.Observe(new CounterSample(CounterNameMap.IlBytesJitted, "IL Bytes Jitted", "Mean", 32_768d, null, new Dictionary<string, object?>()));

        var snapshot = aggregator.Build();

        Assert.NotNull(snapshot);
        Assert.Equal(262_144, snapshot!.LohSizeBytes);
        Assert.Equal(12.5d, snapshot.GcPauseTimePercentage);
        Assert.Equal(48.5d, snapshot.HeapFragmentationPercentage);
        Assert.Equal(7, snapshot.ActiveTimerCount);
        Assert.Equal(321, snapshot.MethodsJittedCount);
        Assert.Equal(32_768, snapshot.IlBytesJitted);
        Assert.Null(snapshot.FinalizationQueueLength);
    }
}
