namespace A2G.ServiceStats.Models;

public sealed record ProcessIdentity(
    int Pid,
    string ProcessName,
    DateTimeOffset? StartTime,
    TimeSpan? Uptime,
    bool IsPublishedDotNetProcess,
    bool IsAttachableCandidate,
    string? AttachabilityReason);
