using CleanSolution.Core.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Interfaces.Repositories
{
    public interface ILogEventRepository
    {
        public Task<IEnumerable<LogEvent>> GetEvents(Guid id, string objectType, int? version = null, DateTime? actTime = null);
    }
}
