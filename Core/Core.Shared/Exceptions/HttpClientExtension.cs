using System.Net.Http.Json;
using System.Text.Json;

namespace Core.Shared.Exceptions;

public static class HttpClientExtension
{
    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(
        this HttpClient http,
        string url,
        TRequest request,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        using var content = JsonContent.Create(request, options: options);
        using var response = await http.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Request failed [{response.StatusCode}]: {errorBody}");
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(options, cancellationToken);
    }
}