using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
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
            services.AddSwaggerGen(c =>
            {
                // DTO კლასის სახელების დაგენერირების წესის განსაზღვრა
                c.CustomSchemaIds(x => x.FullName[(x.FullName.LastIndexOf('.') + 1)..].Replace('+', '.'));

                // ავტორიზაციის წესების განსაზღვრა
                var jwtSecurityScheme = new OpenApiSecurityScheme
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
                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });

                // მეთოდების დახარისხება სხვადასხვა სექციებად
                foreach (var name in options)
                {
                    c.SwaggerDoc(name: name, new OpenApiInfo
                    {
                        Title = "CleanSolution.Presentation.WebApi",
                        Version = "v1.0",
                        Description = "ASP.NET Core Web API Clean Architecture",
                        Contact = new OpenApiContact
                        {
                            Name = "test",
                            Email = "test@mail.com",
                            Url = new Uri("http://test.com/")
                        }
                    });


                }
            });
        }
        // middleware
        public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app, params string[] options)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var name in options)
                {
                    c.SwaggerEndpoint(
                        url: $"{name}/swagger.json",
                        name: name);
                }
            });

            return app;
        }
    }
}