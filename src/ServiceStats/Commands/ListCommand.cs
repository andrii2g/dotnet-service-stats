using A2G.ServiceStats.Discovery;
using A2G.ServiceStats.Models;
using A2G.ServiceStats.Output;
using System.CommandLine;

namespace A2G.ServiceStats.Commands;

internal static class ListCommand
{
    public static Command Build(
        ProcessDiscoveryService processDiscoveryService,
        ConsoleProcessListRenderer consoleRenderer,
        JsonOutputRenderer jsonOutputRenderer)
    {
        var includeAllOption = new Option<bool>("--all")
        {
            Description = "Show all local processes, including non-.NET processes."
        };
        var jsonOption = new Option<bool>("--json")
        {
            Description = "Emit JSON instead of a table."
        };

        var command = new Command("list", "Show local processes that are likely attachable .NET processes.");
        command.Options.Add(includeAllOption);
        command.Options.Add(jsonOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            try
            {
                var includeAll = parseResult.GetValue(includeAllOption);
                var asJson = parseResult.GetValue(jsonOption);
                var result = await processDiscoveryService.ListAsync(includeAll, cancellationToken);

                if (asJson)
                {
                    var envelope = new ProcessListEnvelope(
                        SchemaVersion: "1.0",
                        CapturedAtUtc: DateTimeOffset.UtcNow,
                        IncludeAll: includeAll,
                        SkippedProcessCount: result.SkippedProcessCount,
                        Processes: result.Processes.Select(ProcessListItem.FromIdentity).ToArray());

                    await jsonOutputRenderer.WriteAsync(Console.Out, envelope, cancellationToken);
                }
                else
                {
                    consoleRenderer.Render(result.Processes, includeAll);
                }

                return ExitCodes.Success;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return ExitCodes.UnexpectedError;
            }
        });

        return command;
    }
}
