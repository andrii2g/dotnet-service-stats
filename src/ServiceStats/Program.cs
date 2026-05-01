using A2G.ServiceStats.Commands;
using A2G.ServiceStats.Diagnostics;
using A2G.ServiceStats.Discovery;
using A2G.ServiceStats.Output;
using System.CommandLine;

namespace A2G.ServiceStats;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var attachabilityChecker = new AttachabilityChecker();
        var windowsServiceResolver = new WindowsServiceResolver();
        var processDiscoveryService = new ProcessDiscoveryService(attachabilityChecker, windowsServiceResolver);
        var eventCounterParser = new EventCounterParser();
        var counterCaptureService = new CounterCaptureService(eventCounterParser);
        var jsonOutputRenderer = new JsonOutputRenderer();
        var consoleProcessListRenderer = new ConsoleProcessListRenderer();
        var consoleSnapshotRenderer = new ConsoleSnapshotRenderer();

        var root = new RootCommand("Local .NET service diagnostics snapshot tool.");
        root.Subcommands.Add(ListCommand.Build(processDiscoveryService, consoleProcessListRenderer, jsonOutputRenderer));
        root.Subcommands.Add(SnapCommand.Build(processDiscoveryService, counterCaptureService, consoleSnapshotRenderer, jsonOutputRenderer));

        if (args.Length == 0)
        {
            return await root.Parse("--help").InvokeAsync();
        }

        return await root.Parse(args).InvokeAsync();
    }
}
