namespace A2G.ServiceStats.Output;

internal static class TimeFormatter
{
    public static string Format(TimeSpan? value) => value?.ToString(@"hh\:mm\:ss") ?? "n/a";
}
