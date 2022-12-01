namespace System.Text.Json;
public static partial class JsonSerializerExtension
{
    public static TValue? DeserializeAnonymous<TValue>(string json, TValue anonymousObject, JsonSerializerOptions? options = default)
        => JsonSerializer.Deserialize<TValue>(json, options);

    public static ValueTask<TValue?> DeserializeAnonymousAsync<TValue>(Stream stream, TValue anonymousObject, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken);
}