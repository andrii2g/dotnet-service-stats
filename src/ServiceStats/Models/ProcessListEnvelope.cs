namespace A2G.ServiceStats.Models;

public sealed record ProcessListEnvelope(
    string SchemaVersion,
    DateTimeOffset CapturedAtUtc,
    bool IncludeAll,
    int SkippedProcessCount,
    IReadOnlyList<ProcessListItem> Processes);

public sealed record ProcessListItem(
    int Pid,
    string ProcessName,
    DateTimeOffset? StartTimeUtc,
    TimeSpan? Uptime,
    bool IsPublishedDotNetProcess,
    bool IsAttachableCandidate,
    string? AttachabilityReason)
{
    public static ProcessListItem FromIdentity(ProcessIdentity identity)
        => new(
            identity.Pid,
            identity.ProcessName,
            identity.StartTime,
            identity.Uptime,
            identity.IsPublishedDotNetProcess,
            identity.IsAttachableCandidate,
            identity.AttachabilityReason);
}
