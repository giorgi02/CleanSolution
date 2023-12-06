using Core.Application.Exceptions;
using Core.Application.Interfaces.Services;
using Core.Application.Localize;
using Core.Domain.Models;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace Infrastructure.Messaging.RequestServices;
internal class CheckPersonsService : ICheckPersonsService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;
    private readonly IStringLocalizer<Resource> _localizer;

    public CheckPersonsService(IHttpClientFactory httpClientFactory, IStringLocalizer<Resource> localizer)
    {
        _httpClient = httpClientFactory.CreateClient("HrPortal");
        _localizer = localizer;
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<Employee> GetPerson(string personalNumber, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"api/people?personalNumber={personalNumber}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new EntityNotFoundException(_localizer["person_not_found"]);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            throw new EntityNotFoundException(_localizer["person_not_found"]);

        return JsonSerializer.Deserialize<Employee>(content, _options)!;
    }
}