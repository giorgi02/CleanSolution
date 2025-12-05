using Core.Domain.Models;

namespace Core.Application.Interfaces.Services;

public interface IMessagingService
{
    Task EmployeeCreated(Employee employee);
}
