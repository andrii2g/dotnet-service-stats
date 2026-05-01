namespace A2G.ServiceStats.Discovery;

internal static class ProcessNameMatcher
{
    public static string NormalizeInput(string value)
    {
        var trimmed = value.Trim();
        return trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? trimmed[..^4]
            : trimmed;
    }

    public static bool Matches(string input, string processName)
    {
        var normalizedInput = NormalizeInput(input);
        var normalizedProcess = NormalizeInput(processName);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.OrdinalIgnoreCase;

        return string.Equals(normalizedInput, normalizedProcess, comparison);
    }
}
