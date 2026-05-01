using A2G.ServiceStats.Diagnostics;

namespace A2G.ServiceStats.Tests.Diagnostics;

public sealed class ProcessMetricsCollectorTests
{
    [Fact]
    public void ComputeCpuPercent_NormalizesAcrossProcessorCount()
    {
        var result = ProcessMetricsCollector.ComputeCpuPercent(
            deltaProcessCpu: TimeSpan.FromSeconds(4),
            deltaWallClock: TimeSpan.FromSeconds(2),
            processorCount: 4);

        Assert.Equal(50d, result);
    }
}
