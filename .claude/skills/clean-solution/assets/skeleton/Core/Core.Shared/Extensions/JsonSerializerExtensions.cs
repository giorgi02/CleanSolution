using System.Text.Json;

namespace Core.Shared.Extensions;

public static class JsonSerializerExtensions
{
    /// <summary>
    /// Deserializes into the shape of an anonymous-type prototype:
    /// <c>JsonSerializerExtensions.DeserializeAnonymous(json, new { Id = 0, Name = "" })</c>.
    /// </summary>
    public static TValue? DeserializeAnonymous<TValue>(string json, TValue _, JsonSerializerOptions? options = default)
        => JsonSerializer.Deserialize<TValue>(json, options);

    public static ValueTask<TValue?> DeserializeAnonymousAsync<TValue>(Stream stream, TValue _, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken);
}
