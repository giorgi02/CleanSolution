using CleanSolution.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanSolution.Infrastructure.Persistence
{
    internal class DataContext : DbContext
    {
        public DbSet<Employee> Employes { get; set; }
        public DbSet<Position> Positions { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
    }
}
