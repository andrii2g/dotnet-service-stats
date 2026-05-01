using A2G.ServiceStats.Diagnostics;

namespace A2G.ServiceStats.Tests.Diagnostics;

public sealed class EventCounterParserTests
{
    [Fact]
    public void TryParse_ParsesKnownFields()
    {
        var parser = new EventCounterParser();
        var payload = new Dictionary<string, object?>
        {
            ["Name"] = "gc-heap-size",
            ["DisplayName"] = "GC Heap Size",
            ["CounterType"] = "Mean",
            ["Mean"] = 1234d,
            ["Metadata"] = new Dictionary<string, object?>()
        };

        var parsed = parser.TryParse(payload, out var sample);

        Assert.True(parsed);
        Assert.NotNull(sample);
        Assert.Equal("gc-heap-size", sample!.Name);
        Assert.Equal(1234d, sample.Mean);
    }
}
