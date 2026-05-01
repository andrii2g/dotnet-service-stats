namespace A2G.ServiceStats.Output;

internal static class SizeFormatter
{
    public static string Format(long? bytes)
    {
        if (bytes is null)
        {
            return "n/a";
        }

        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes.Value;
        var suffixIndex = 0;

        while (value >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1024;
            suffixIndex++;
        }

        return $"{value:0.#} {suffixes[suffixIndex]}";
    }
}
