global using Microsoft.EntityFrameworkCore;
using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Infrastructure.Persistence.Implementations.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanSolution.Infrastructure.Persistence;
public static class ServiceExtensions
{
    public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();

        services.AddDbContext<DataContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }
}