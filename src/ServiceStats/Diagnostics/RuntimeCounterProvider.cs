using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using System.Diagnostics.Tracing;

namespace A2G.ServiceStats.Diagnostics;

internal static class RuntimeCounterProvider
{
    public static EventPipeProvider Create()
        => new(
            "System.Runtime",
            EventLevel.Informational,
            keywords: 0,
            arguments: new Dictionary<string, string?>()
            {
                ["EventCounterIntervalSec"] = "1"
            });
}
