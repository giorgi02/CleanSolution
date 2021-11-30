using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Core.Domain.Helpers;

namespace CleanSolution.Infrastructure.Persistence.Implementations.Repositories;
internal class LogEventRepository : ILogEventRepository
{
    private readonly DataContext context;

    public LogEventRepository(DataContext context)
    {
        this.context = context;
    }
    public async Task<IEnumerable<LogEvent>> GetEvents(Guid id, string objectType, int? version = null, DateTime? actTime = null)
    {
        return await context.LogEvents
            .Where(x => x.ObjectId == id &&
                x.ObjectType == objectType &&
                (version == null || x.Version >= version) &&
                (actTime == null || x.ActTime >= actTime))
            .ToListAsync();
    }
}