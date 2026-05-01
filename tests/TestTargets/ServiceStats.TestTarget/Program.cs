using System.Collections.Concurrent;

namespace A2G.ServiceStats.TestTarget;

public static class Marker;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var mode = ParseMode(args);
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        Console.WriteLine(Environment.ProcessId);
        await Console.Out.FlushAsync();

        var workers = new List<Task>
        {
            RunAsync(mode, cts.Token)
        };

        try
        {
            await Task.WhenAll(workers);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static string ParseMode(string[] args)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], "--mode", StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return "idle";
    }

    private static async Task RunAsync(string mode, CancellationToken cancellationToken)
    {
        var queue = new ConcurrentQueue<byte[]>();

        while (!cancellationToken.IsCancellationRequested)
        {
            switch (mode.ToLowerInvariant())
            {
                case "alloc":
                    queue.Enqueue(new byte[256 * 1024]);
                    while (queue.Count > 32 && queue.TryDequeue(out _))
                    {
                    }
                    break;

                case "exceptions":
                    try
                    {
                        throw new InvalidOperationException("Synthetic exception.");
                    }
                    catch
                    {
                    }
                    break;

                case "threadpool":
                    await Task.Run(() =>
                    {
                        Thread.SpinWait(100_000);
                    }, cancellationToken);
                    break;

                case "idle":
                default:
                    break;
            }

            await Task.Delay(100, cancellationToken);
        }
    }
}
