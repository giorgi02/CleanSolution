using Core.Domain.Models;

namespace Core.Application.Interfaces.Services;
public interface ICheckPersonsService
{
    Task<Employee> GetPerson(string personalNumber, CancellationToken cancellationToken);
}
