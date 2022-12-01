using System.Text.Json;

namespace Core.Domain.Extensions;
public static class JsonSerializerExtensions
{
    public static TValue? DeserializeAnonymousType<TValue>(string json, TValue anonymousObject, JsonSerializerOptions? options = default)
        => JsonSerializer.Deserialize<TValue>(json, options);

    public static ValueTask<TValue?> DeserializeAnonymousTypeAsync<TValue>(Stream stream, TValue anonymousObject, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken);
}