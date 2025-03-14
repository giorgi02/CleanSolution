using Core.Application.Interfaces.Services;
using Core.Domain.Models;
using Core.Shared;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.Messaging.RequestServices;
internal class HrPortalServices(HttpClient httpClient, ILogger<HrPortalServices> logger) : IHrPortalServices
{
    public async Task<Employee?> GetEmployee(string personalNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<Employee>($"api/employees/{personalNumber}", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "HrPortalServices.GetEmployee პრობლემა HrPortal სერვისის გამოძახებისას");
            throw new ApiValidationException("პრობლემა HrPortal სერვისის გამოძახებისას");
        }
    }
}