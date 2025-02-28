using Core.Domain.Models;
using Infrastructure.Persistence.Configurations;

namespace Infrastructure.Persistence;
internal class DataContext(DbContextOptions<DataContext> options)
    : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TodoItemConfiguration());
    }
}