using System.Text.Json;
using System.Text.Json.Serialization;

namespace A2G.ServiceStats.Output;

internal sealed class JsonOutputRenderer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);

    public Task WriteAsync<T>(TextWriter writer, T value, CancellationToken cancellationToken)
        => writer.WriteAsync(Serialize(value));
}
