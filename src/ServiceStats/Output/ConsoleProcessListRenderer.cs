using A2G.ServiceStats.Models;
using Spectre.Console;

namespace A2G.ServiceStats.Output;

internal sealed class ConsoleProcessListRenderer
{
    public void Render(IReadOnlyList<ProcessIdentity> processes, bool includeAll)
    {
        var console = AnsiConsole.Create(new AnsiConsoleSettings());
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("PID");
        table.AddColumn("Name");
        table.AddColumn("Uptime");
        table.AddColumn("Status");

        foreach (var process in processes)
        {
            table.AddRow(
                process.Pid.ToString(),
                process.ProcessName,
                TimeFormatter.Format(process.Uptime),
                process.IsAttachableCandidate ? "attachable" : "not attachable");
        }

        console.Write(new Rule(includeAll ? "Local processes" : "Attachable .NET processes"));
        console.Write(table);
    }
}
