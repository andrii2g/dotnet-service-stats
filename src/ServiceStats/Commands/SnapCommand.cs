using A2G.ServiceStats.Diagnostics;
using A2G.ServiceStats.Discovery;
using A2G.ServiceStats.Models;
using A2G.ServiceStats.Output;
using System.CommandLine;

namespace A2G.ServiceStats.Commands;

internal static class SnapCommand
{
    public static Command Build(
        ProcessDiscoveryService processDiscoveryService,
        CounterCaptureService counterCaptureService,
        ConsoleSnapshotRenderer consoleRenderer,
        JsonOutputRenderer jsonOutputRenderer)
    {
        var pidOption = new Option<int?>("--pid")
        {
            Description = "Target exact process id."
        };
        var nameOption = new Option<string?>("--name")
        {
            Description = "Target process by process name."
        };
        var serviceOption = new Option<string?>("--service")
        {
            Description = "Target Windows service name."
        };
        var durationOption = new Option<int>("--duration")
        {
            Description = "EventPipe collection duration in seconds.",
            DefaultValueFactory = _ => 3
        };
        var timeoutOption = new Option<int>("--timeout")
        {
            Description = "Total command timeout in seconds.",
            DefaultValueFactory = _ => 15
        };
        var jsonOption = new Option<bool>("--json")
        {
            Description = "Emit JSON output."
        };
        var noColorOption = new Option<bool>("--no-color")
        {
            Description = "Disable ANSI styling."
        };
        var verboseOption = new Option<bool>("--verbose")
        {
            Description = "Include diagnostic warnings and provider details."
        };

        durationOption.Validators.Add(result =>
        {
            var value = result.GetValueOrDefault<int>();
            if (value < 1 || value > 30)
            {
                result.AddError("--duration must be between 1 and 30 seconds.");
            }
        });

        timeoutOption.Validators.Add(result =>
        {
            var value = result.GetValueOrDefault<int>();
            if (value < 3 || value > 120)
            {
                result.AddError("--timeout must be between 3 and 120 seconds.");
            }
        });

        var command = new Command("snap", "Attach to one local .NET process, collect a short diagnostics sample, and print a normalized snapshot.");
        command.Options.Add(pidOption);
        command.Options.Add(nameOption);
        command.Options.Add(serviceOption);
        command.Options.Add(durationOption);
        command.Options.Add(timeoutOption);
        command.Options.Add(jsonOption);
        command.Options.Add(noColorOption);
        command.Options.Add(verboseOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var asJson = parseResult.GetValue(jsonOption);
            var verbose = parseResult.GetValue(verboseOption);

            try
            {
                var target = await ResolveTargetAsync(
                    processDiscoveryService,
                    parseResult.GetValue(pidOption),
                    parseResult.GetValue(nameOption),
                    parseResult.GetValue(serviceOption),
                    cancellationToken);

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(parseResult.GetValue(timeoutOption)));

                var captureResult = await counterCaptureService.CaptureAsync(
                    target,
                    TimeSpan.FromSeconds(parseResult.GetValue(durationOption)),
                    timeoutCts.Token);

                if (asJson)
                {
                    await jsonOutputRenderer.WriteAsync(Console.Out, captureResult.Snapshot, cancellationToken);
                }
                else
                {
                    consoleRenderer.Render(captureResult.Snapshot, !parseResult.GetValue(noColorOption), verbose);
                }

                return captureResult.ExitCode;
            }
            catch (DiagnosticsException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return ex.ExitCode;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                Console.Error.WriteLine("The capture timed out before a useful snapshot could be produced.");
                return ExitCodes.Timeout;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(verbose ? ex.ToString() : ex.Message);
                return ExitCodes.UnexpectedError;
            }
        });

        return command;
    }

    private static async Task<ProcessIdentity> ResolveTargetAsync(
        ProcessDiscoveryService processDiscoveryService,
        int? pid,
        string? name,
        string? service,
        CancellationToken cancellationToken)
    {
        var populatedOptions = new object?[] { pid, name, service }
            .Count(value => value is int || !string.IsNullOrWhiteSpace(value as string));

        if (populatedOptions != 1)
        {
            throw new DiagnosticsException(ExitCodes.UsageError, "Exactly one of --pid, --name, or --service must be provided.");
        }

        if (pid is int concretePid)
        {
            return await processDiscoveryService.ResolveByPidAsync(concretePid, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            return await processDiscoveryService.ResolveByNameAsync(name, cancellationToken);
        }

        return await processDiscoveryService.ResolveByServiceAsync(service!, cancellationToken);
    }
}
