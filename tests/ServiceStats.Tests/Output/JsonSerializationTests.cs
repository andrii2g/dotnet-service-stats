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
}
