using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CleanSolution.Infrastructure.Persistence.Implementations.Repositories
{
    internal class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(DataContext context) : base(context) { }

        private IQueryable<Employee> Including =>
            this.context.Employes.Include(x => x.Position);

        public async Task<Pagination<Employee>> FilterAsync(int pageIndex, int pageSize, string privateNumber = null, string firatName = null, string lastName = null, Gender? gender = null)
        {
            var employees = this.Including.Where(x =>
                (privateNumber == null || x.PrivateNumber == privateNumber) &&
                (firatName == null || x.FirstName == firatName) &&
                (lastName == null || x.LastName == lastName) &&
                (gender == null || x.Gender == gender)
            );

            return await Pagination<Employee>.CreateAsync(employees, pageIndex, pageSize);
        }

        public async Task<Pagination<Employee>> SearchAsync(int pageIndex, int pageSize, string firatName = null, string lastName = null)
        {
            var employees = this.Including.Where(x => x.FirstName == firatName || x.LastName == lastName);

            return await Pagination<Employee>.CreateAsync(employees, pageIndex, pageSize);
        }
    }
}
