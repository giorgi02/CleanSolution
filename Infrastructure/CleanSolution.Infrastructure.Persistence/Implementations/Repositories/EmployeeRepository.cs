using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CleanSolution.Infrastructure.Persistence.Implementations.Repositories
{
    internal class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(DataContext context) : base(context) { }


        private IQueryable<Employee> Including =>
            _context.Employes.Include(x => x.Position);


        public override async Task<Employee> ReadAsync(Guid id)
        {
            return await this.Including.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Pagination<Employee>> FilterAsync(int pageIndex, int pageSize, string privateNumber = null, string firstName = null, string lastName = null, Gender? gender = null, Language? language = null)
        {
            var employees = this.Including.Where(x =>
                (privateNumber == null || x.PrivateNumber == privateNumber) &&
                (firstName == null || x.FirstName == firstName) &&
                (lastName == null || x.LastName == lastName) &&
                (gender == null || x.Gender == gender) &&
                (language == null || x.Language.HasFlag(language))
            );

            return await Pagination<Employee>.CreateAsync(employees, pageIndex, pageSize);
        }

        public async Task<Pagination<Employee>> SearchAsync(int pageIndex, int pageSize, string text)
        {
            var employees = this.Including.Where(x => x.PrivateNumber == text || x.FirstName == text || x.LastName == text);

            return await Pagination<Employee>.CreateAsync(employees, pageIndex, pageSize);
        }
    }
}
