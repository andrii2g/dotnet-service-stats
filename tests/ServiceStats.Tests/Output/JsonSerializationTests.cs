using A2G.ServiceStats.Models;
using A2G.ServiceStats.Output;
using System.Text.Json;

namespace A2G.ServiceStats.Tests.Output;

public sealed class JsonSerializationTests
{
    [Fact]
    public void Serialize_UsesCamelCaseForProcessList()
    {
        var renderer = new JsonOutputRenderer();
        var json = renderer.Serialize(new ProcessListEnvelope(
            "1.0",
            DateTimeOffset.Parse("2026-05-02T20:15:30Z"),
            false,
            1,
            [
                new ProcessListItem(1234, "Orders.Api", null, null, true, true, null)
            ]));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.True(root.TryGetProperty("schemaVersion", out _));
        Assert.True(root.TryGetProperty("skippedProcessCount", out _));
    }

    [Fact]
    public void Serialize_WritesStatusAsString()
    {
        var renderer = new JsonOutputRenderer();
        var json = renderer.Serialize(new ServiceStatsSnapshot(
            "1.0",
            DateTimeOffset.Parse("2026-05-02T20:15:30Z"),
            TimeSpan.FromSeconds(3),
            SnapshotStatus.Partial,
            new ProcessSnapshot(1, "dotnet", null, null, null, null, null, null, null, null, null),
            null,
            []));

        using var document = JsonDocument.Parse(json);
        Assert.Equal("Partial", document.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public void Serialize_WritesRuntimeDeepMetricsInCamelCase()
    {
        var renderer = new JsonOutputRenderer();
        var json = renderer.Serialize(new ServiceStatsSnapshot(
            "1.0",
            DateTimeOffset.Parse("2026-05-02T20:15:30Z"),
            TimeSpan.FromSeconds(3),
            SnapshotStatus.Complete,
            new ProcessSnapshot(1234, "dotnet", null, null, null, null, null, null, null, null, null),
            new RuntimeSnapshot(
                RuntimeProviderName: "System.Runtime",
                GcHeapSizeBytes: 1_048_576,
                LohSizeBytes: 262_144,
                AllocationRateBytesPerSecond: 2048,
                GcPauseTimePercentage: 12.5d,
                HeapFragmentationPercentage: 48.5d,
                ExceptionRatePerSecond: null,
                Gen0CollectionsPerSecond: null,
                Gen1CollectionsPerSecond: null,
                Gen2CollectionsPerSecond: null,
                FinalizationQueueLength: null,
                ActiveTimerCount: 7,
                MethodsJittedCount: 321,
                IlBytesJitted: 32_768,
                ThreadPoolThreadCount: 16,
                ThreadPoolQueueLength: 0,
                MonitorLockContentionCountPerSecond: null,
                RawCounters: new Dictionary<string, double>
                {
                    ["loh-size"] = 262_144d
                }),
            []));

        using var document = JsonDocument.Parse(json);
        var runtime = document.RootElement.GetProperty("runtime");
        Assert.Equal(262_144, runtime.GetProperty("lohSizeBytes").GetInt64());
        Assert.Equal(12.5d, runtime.GetProperty("gcPauseTimePercentage").GetDouble());
        Assert.Equal(48.5d, runtime.GetProperty("heapFragmentationPercentage").GetDouble());
        Assert.Equal(7, runtime.GetProperty("activeTimerCount").GetInt32());
        Assert.Equal(321, runtime.GetProperty("methodsJittedCount").GetInt32());
        Assert.Equal(32_768, runtime.GetProperty("ilBytesJitted").GetInt64());
    }
}
