using A2G.ServiceStats.Models;
using A2G.ServiceStats.Output;
using System.Text.Json;

namespace A2G.ServiceStats.Tests.Discovery;

public sealed class ProcessIdentitySerializationTests
{
    [Fact]
    public void ProcessListJson_DoesNotExposeCommandLine()
    {
        var renderer = new JsonOutputRenderer();
        var json = renderer.Serialize(new ProcessListEnvelope(
            "1.0",
            DateTimeOffset.Parse("2026-05-02T20:15:30Z"),
            false,
            0,
            [
                new ProcessListItem(
                    1234,
                    "orders-api",
                    DateTimeOffset.Parse("2026-05-02T20:14:30Z"),
                    TimeSpan.FromMinutes(1),
                    true,
                    true,
                    null)
            ]));

        using var document = JsonDocument.Parse(json);
        var process = document.RootElement.GetProperty("processes")[0];
        Assert.False(process.TryGetProperty("commandLine", out _));
    }
}
