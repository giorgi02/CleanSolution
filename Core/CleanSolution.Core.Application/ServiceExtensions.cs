using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CleanSolution.Core.Application
{
    public static class ServiceExtensions
    {
        public static void AddApplicatonLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
