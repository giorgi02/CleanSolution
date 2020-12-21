using CleanSolution.Core.Application.Interfaces.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanSolution.Infrastructure.Identity
{
    public static class ServiceExtensions
    {
        public static void AddIdentityLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAccountManager, AccountManager>();
        }
    }
}
