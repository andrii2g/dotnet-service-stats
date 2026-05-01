namespace A2G.ServiceStats.Diagnostics;

internal sealed record CounterSample(
    string Name,
    string? DisplayName,
    string? CounterType,
    double? Mean,
    double? Increment,
    IDictionary<string, object?> Metadata);
