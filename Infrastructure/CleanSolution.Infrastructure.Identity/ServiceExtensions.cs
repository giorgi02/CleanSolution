using CleanSolution.Core.Application.Interfaces.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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
