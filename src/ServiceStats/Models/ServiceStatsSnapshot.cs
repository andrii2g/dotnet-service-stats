namespace A2G.ServiceStats.Models;

public sealed record ServiceStatsSnapshot(
    string SchemaVersion,
    DateTimeOffset CapturedAtUtc,
    TimeSpan CaptureDuration,
    SnapshotStatus Status,
    ProcessSnapshot Process,
    RuntimeSnapshot? Runtime,
    IReadOnlyList<string> Warnings);
