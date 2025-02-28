using Core.Domain.Models;

namespace Core.Application.Interfaces.Services;
public interface IHrPortalServices
{
    Task<TodoItem?> GetEmployee(string personalNumber, CancellationToken cancellationToken);
}
