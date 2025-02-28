using Core.Application.Interfaces.Services;
using Core.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.RequestServices;
internal class HrPortalServices(HttpClient httpClient, ILogger<HrPortalServices> logger) : IHrPortalServices
{
    public Task<TodoItem?> GetEmployee(string personalNumber, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}