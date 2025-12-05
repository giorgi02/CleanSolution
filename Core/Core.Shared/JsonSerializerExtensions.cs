using System.Text.Json;

namespace Core.Shared;

public static partial class JsonSerializerExtensions
{
    public static TValue? DeserializeAnonymous<TValue>(string json, TValue _, JsonSerializerOptions? options = default)
        => JsonSerializer.Deserialize<TValue>(json, options);

    public static ValueTask<TValue?> DeserializeAnonymousAsync<TValue>(Stream stream, TValue _, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken);
}