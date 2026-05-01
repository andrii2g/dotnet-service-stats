using A2G.ServiceStats.Models;

namespace A2G.ServiceStats.Diagnostics;

internal sealed class RuntimeCounterAggregator
{
    private readonly Dictionary<string, double> _rawCounters = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<double>> _rates = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _gauges = new(StringComparer.OrdinalIgnoreCase);

    public void Observe(CounterSample sample)
    {
        if (!CounterNameMap.RequiredCounters.Contains(sample.Name))
        {
            return;
        }

        if (sample.Increment is double increment)
        {
            _rawCounters[sample.Name] = increment;
            GetRateBucket(sample.Name).Add(increment);
        }
        else if (sample.Mean is double mean)
        {
            _rawCounters[sample.Name] = mean;
            _gauges[sample.Name] = mean;
        }
    }

    public RuntimeSnapshot? Build()
    {
        if (_rawCounters.Count == 0 && _gauges.Count == 0)
        {
            return null;
        }

        return new RuntimeSnapshot(
            RuntimeProviderName: "System.Runtime",
            GcHeapSizeBytes: AsLongGauge(CounterNameMap.GcHeapSize),
            LohSizeBytes: AsLongGauge(CounterNameMap.LohSize),
            AllocationRateBytesPerSecond: GetGauge(CounterNameMap.AllocationRate) ?? GetAverageRate(CounterNameMap.AllocationRate),
            GcPauseTimePercentage: GetGauge(CounterNameMap.TimeInGc),
            HeapFragmentationPercentage: GetGauge(CounterNameMap.GcFragmentation),
            ExceptionRatePerSecond: GetAverageRate(CounterNameMap.ExceptionCount) ?? GetGauge(CounterNameMap.ExceptionCount),
            Gen0CollectionsPerSecond: GetAverageRate(CounterNameMap.Gen0Count),
            Gen1CollectionsPerSecond: GetAverageRate(CounterNameMap.Gen1Count),
            Gen2CollectionsPerSecond: GetAverageRate(CounterNameMap.Gen2Count),
            // System.Runtime does not expose finalization queue length as an EventCounter.
            FinalizationQueueLength: null,
            ActiveTimerCount: AsIntGauge(CounterNameMap.ActiveTimerCount),
            MethodsJittedCount: AsIntGauge(CounterNameMap.MethodsJittedCount),
            IlBytesJitted: AsLongGauge(CounterNameMap.IlBytesJitted),
            ThreadPoolThreadCount: AsIntGauge(CounterNameMap.ThreadPoolThreadCount),
            ThreadPoolQueueLength: AsIntGauge(CounterNameMap.ThreadPoolQueueLength),
            MonitorLockContentionCountPerSecond: GetAverageRate(CounterNameMap.MonitorLockContentionCount),
            RawCounters: new Dictionary<string, double>(_rawCounters, StringComparer.OrdinalIgnoreCase));
    }

    private List<double> GetRateBucket(string name)
    {
        if (!_rates.TryGetValue(name, out var values))
        {
            values = [];
            _rates[name] = values;
        }

        return values;
    }

    private double? GetAverageRate(string name)
        => _rates.TryGetValue(name, out var values) && values.Count > 0
            ? values.Average()
            : null;

    private double? GetGauge(string name)
        => _gauges.TryGetValue(name, out var value) ? value : null;

    private long? AsLongGauge(string name)
        => GetGauge(name) is double value ? (long)Math.Round(value) : null;

    private int? AsIntGauge(string name)
        => GetGauge(name) is double value ? (int)Math.Round(value) : null;
}
