using Core.Application.Exceptions;
using Core.Application.Interfaces.Services;
using Core.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.Messaging.RequestServices;
internal class HrPortalServices : IHrPortalServices
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HrPortalServices> _logger;

    public HrPortalServices(HttpClient httpClient, ILogger<HrPortalServices> logger) =>
        (_httpClient, _logger) = (httpClient, logger);


    public async Task<Employee?> GetEmployee(string personalNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Employee>($"api/employees/{personalNumber}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HrPortalServices.GetEmployee პრობლემა HrPortal სერვისის გამოძახებისას");
            throw new EntityValidationException("პრობლემა HrPortal სერვისის გამოძახებისას");
        }
    }
}