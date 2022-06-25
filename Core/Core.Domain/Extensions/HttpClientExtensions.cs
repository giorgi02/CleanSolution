using System.Text.Json;

namespace Core.Domain.Extensions;

public static class HttpClientExtensions
{
    /// <summary>
    /// HttpClient ის response-ის დესერიალიზება სასურველ ფორმატში
    /// </summary>
    public static async Task<T?> ReadContentAs<T>(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase}");

        var dataAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonSerializer.Deserialize<T>(dataAsString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
