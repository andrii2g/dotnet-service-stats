using Microsoft.Diagnostics.NETCore.Client;

namespace A2G.ServiceStats.Diagnostics;

internal sealed class AttachabilityChecker
{
    public IReadOnlySet<int> GetPublishedProcessIds()
    {
        try
        {
            return DiagnosticsClient.GetPublishedProcesses().ToHashSet();
        }
        catch
        {
            return new HashSet<int>();
        }
    }

    public bool IsPublished(int pid) => GetPublishedProcessIds().Contains(pid);
}
