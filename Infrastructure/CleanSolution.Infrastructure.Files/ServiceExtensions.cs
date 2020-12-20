using AutoMapper.Configuration;
using CleanSolution.Core.Application.Interfaces.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanSolution.Infrastructure.Files
{
    public static class ServiceExtensions
    {
        public static void AddFilesLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFileManager, FileManager>();
        }
    }
}
