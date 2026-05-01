using System.Collections;

namespace A2G.ServiceStats.Diagnostics;

internal sealed class EventCounterParser
{
    public bool TryParse(object? payload, out CounterSample? sample)
    {
        sample = null;

        if (payload is not IDictionary payloadDictionary)
        {
            return false;
        }

        var name = GetString(payloadDictionary, "Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        sample = new CounterSample(
            Name: name,
            DisplayName: GetString(payloadDictionary, "DisplayName"),
            CounterType: GetString(payloadDictionary, "CounterType"),
            Mean: GetDouble(payloadDictionary, "Mean"),
            Increment: GetDouble(payloadDictionary, "Increment"),
            Metadata: GetMetadata(payloadDictionary));

        return true;
    }

    private static IDictionary<string, object?> GetMetadata(IDictionary payload)
    {
        if (payload["Metadata"] is IDictionary metadataDictionary)
        {
            return ToDictionary(metadataDictionary);
        }

        return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    private static string? GetString(IDictionary payload, string key)
        => payload.Contains(key) ? payload[key]?.ToString() : null;

    private static double? GetDouble(IDictionary payload, string key)
    {
        if (!payload.Contains(key) || payload[key] is not { } value)
        {
            return null;
        }

        return value switch
        {
            float single => single,
            double dbl => dbl,
            decimal dec => (double)dec,
            int integer => integer,
            long longInteger => longInteger,
            string s when double.TryParse(s, out var parsed) => parsed,
            _ => null
        };
    }

    private static Dictionary<string, object?> ToDictionary(IDictionary dictionary)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (DictionaryEntry entry in dictionary)
        {
            if (entry.Key is string key)
            {
                result[key] = entry.Value;
            }
        }

        return result;
    }
}
