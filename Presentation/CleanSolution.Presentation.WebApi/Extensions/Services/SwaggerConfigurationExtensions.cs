﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;

namespace Workabroad.Presentation.Admin.Extensions.Services
{
    public static class SwaggerConfigurationExtensions
    {
        // services
        public static void AddSwaggerServices(this IServiceCollection services, params string[] options)
        {
            // Swagger-ის გენერატორის რეგისტრაცია, 1 ან მეტი Swagger დოკუმენტის განსაზღვრა
            services.AddSwaggerGen(s =>
            {
                foreach (var name in options)
                {
                    s.SwaggerDoc(name: name, new OpenApiInfo
                    {
                        Title = "Workabroad Admin",
                        Version = "v1.0",
                        Description = "ASP.NET Core Web API ტექნოლოგიაზე შექმნილი Workabroad სერვისები დასაქმების ხელშეწყობის სახელმწიფო სააგენტოსთვის",
                        Contact = new OpenApiContact
                        {
                            Name = "SESA",
                            Email = "sesa@ssa.gov.ge.com",
                            Url = new Uri("http://ssa.gov.ge/")
                        }
                    });
                }
            });
        }
        // middleware
        public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app, params string[] options)
        {
            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                foreach (var name in options)
                {
                    s.SwaggerEndpoint(
                        url: $"/swagger/{name}/swagger.json",
                        name: name);
                }
            });

            return app;
        }
    }
}