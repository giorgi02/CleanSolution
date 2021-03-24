using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace $safeprojectname$.Implementations.Repositories
{
    internal class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(DataContext context) : base(context) { }

        private IQueryable<Employee> Including =>
            this.context.Employes.Include(x => x.Position);

        public async Task<Pagination<Employee>> Filter(int pageIndex, int pageSize, string firatName, string lastName)
        {
            var employees = this.Including.Where(x => x.FirstName == firatName && x.LastName == lastName);

            return await Pagination<Employee>.CreateAsync(employees, pageIndex, pageSize);
        }

        public async Task<Pagination<Employee>> Search(int pageIndex, int pageSize, string firatName, string lastName)
        {
            var employees = this.Including.Where(x => x.FirstName == firatName || x.LastName == lastName);

            return await Pagination<Employee>.CreateAsync(employees, pageIndex, pageSize);
        }
    }
}
