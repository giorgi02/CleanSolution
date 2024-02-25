using Core.Domain.Models;

namespace Core.Application.Interfaces.Services;
public interface IHrPortalServices
{
    Task<Employee?> GetEmployee(string personalNumber, CancellationToken cancellationToken);
}
