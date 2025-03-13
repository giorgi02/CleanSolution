﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Presentation.WebApi.Extensions.Configurations;
public static class SwaggerConfiguration
{
    private readonly static string[] Options = ["CleanSolution v1"];

    // services
    public static void AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.UseAllOfToExtendReferenceSchemas();

            options.CustomSchemaIds(x => x.FullName?.Replace('+', '.'));

            var securityScheme = new OpenApiSecurityScheme
            {
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Description = "ქვედა ტექსტბოქსში ჩაწერეთ **_მხოლოდ_** თქვენი JWT Bearer token !",

                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };
            options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });

            foreach (var name in Options)
            {
                options.SwaggerDoc(name: name, new OpenApiInfo
                {
                    Title = "CleanSolution.Presentation.WebApi",
                    Version = "v1.0",
                    Description = "ASP.NET Core Web API - CleanSolution",
                    Contact = new OpenApiContact
                    {
                        Name = "test",
                        Email = "test@mail.com",
                        Url = new Uri("http://test.com/")
                    }
                });
            }

            var xmlPath = Path.Combine(AppContext.BaseDirectory, "SwaggerDescription.xml");
            options.IncludeXmlComments(xmlPath);
        });
    }

    // middleware
    public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.InjectStylesheet("/SwaggerDark.css"); 

            foreach (var name in Options)
            {
                c.SwaggerEndpoint(
                    url: $"{name}/swagger.json",
                    name: name);
            }
        });

        return app;
    }
}