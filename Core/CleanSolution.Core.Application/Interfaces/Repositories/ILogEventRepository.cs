using CleanSolution.Core.Domain.Helpers;

namespace CleanSolution.Core.Application.Interfaces.Repositories;
public interface ILogEventRepository
{
    public Task<IEnumerable<LogEvent>> GetEvents(Guid id, string objectType, int? version = null, DateTime? actTime = null);
}
