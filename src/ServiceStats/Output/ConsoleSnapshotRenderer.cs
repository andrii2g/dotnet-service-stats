using A2G.ServiceStats.Models;
using Spectre.Console;

namespace A2G.ServiceStats.Output;

internal sealed class ConsoleSnapshotRenderer
{
    public void Render(ServiceStatsSnapshot snapshot, bool useColor, bool verbose)
    {
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = useColor ? AnsiSupport.Detect : AnsiSupport.No
        });

        console.Write(new Rule("Service stats snapshot"));
        console.Write(CreateTargetGrid(snapshot));
        console.Write(CreateProcessGrid(snapshot.Process));

        if (snapshot.Runtime is not null)
        {
            console.Write(CreateRuntimeGrid(snapshot.Runtime));
        }

        if (snapshot.Status == SnapshotStatus.Partial || (verbose && snapshot.Warnings.Count > 0))
        {
            var warningGrid = new Grid();
            warningGrid.AddColumn();
            warningGrid.AddRow("[yellow]Warnings[/]");
            foreach (var warning in snapshot.Warnings)
            {
                warningGrid.AddRow($"- {warning}");
            }

            console.Write(warningGrid);
        }
    }

    private static Grid CreateTargetGrid(ServiceStatsSnapshot snapshot)
    {
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow("Process:", snapshot.Process.ProcessName);
        grid.AddRow("PID:", snapshot.Process.Pid.ToString());
        grid.AddRow("Uptime:", TimeFormatter.Format(snapshot.Process.Uptime));
        grid.AddRow("Status:", snapshot.Status.ToString());
        return grid;
    }

    private static Grid CreateProcessGrid(ProcessSnapshot snapshot)
    {
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow("CPU:", snapshot.CpuPercent is double cpu ? $"{cpu:F1}%" : "n/a");
        grid.AddRow("Working set:", SizeFormatter.Format(snapshot.WorkingSetBytes));
        grid.AddRow("Private memory:", SizeFormatter.Format(snapshot.PrivateMemoryBytes));
        grid.AddRow("Virtual memory:", SizeFormatter.Format(snapshot.VirtualMemoryBytes));
        grid.AddRow("Handles:", snapshot.HandleCount?.ToString() ?? "n/a");
        grid.AddRow("Threads:", snapshot.ThreadCount?.ToString() ?? "n/a");
        return grid;
    }

    private static Grid CreateRuntimeGrid(RuntimeSnapshot snapshot)
    {
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow("GC heap:", SizeFormatter.Format(snapshot.GcHeapSizeBytes));
        grid.AddRow("Allocation rate:", snapshot.AllocationRateBytesPerSecond is double alloc ? $"{SizeFormatter.Format((long)alloc)}/s" : "n/a");
        grid.AddRow("Exceptions:", snapshot.ExceptionRatePerSecond is double exRate ? $"{exRate:F1}/s" : "n/a");
        grid.AddRow("Gen0 collections:", snapshot.Gen0CollectionsPerSecond is double gen0 ? $"{gen0:F1}/s" : "n/a");
        grid.AddRow("Gen1 collections:", snapshot.Gen1CollectionsPerSecond is double gen1 ? $"{gen1:F1}/s" : "n/a");
        grid.AddRow("Gen2 collections:", snapshot.Gen2CollectionsPerSecond is double gen2 ? $"{gen2:F1}/s" : "n/a");
        grid.AddRow("ThreadPool threads:", snapshot.ThreadPoolThreadCount?.ToString() ?? "n/a");
        grid.AddRow("ThreadPool queue:", snapshot.ThreadPoolQueueLength?.ToString() ?? "n/a");
        grid.AddRow("Lock contentions:", snapshot.MonitorLockContentionCountPerSecond is double locks ? $"{locks:F1}/s" : "n/a");
        return grid;
    }
}
