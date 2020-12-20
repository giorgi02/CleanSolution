using CleanSolution.Core.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CleanSolution.Infrastructure.Persistence
{
    public class DataContext : DbContext
    {
        public DbSet<Employee> Employes { get; set; }
        public DbSet<Position> Positions { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
    }
}
