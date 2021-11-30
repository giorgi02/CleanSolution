using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace CleanSolution.Presentation.WebApi.Extensions.Services;
public static class SwaggerConfigurationService
{
    // services
    public static void AddSwaggerServices(this IServiceCollection services, params string[] options)
    {
        services.AddEndpointsApiExplorer();

        // Swagger-ის გენერატორის რეგისტრაცია, 1 ან მეტი Swagger დოკუმენტის განსაზღვრა
        services.AddSwaggerGen(c =>
        {
            // DTO კლასის სახელების დაგენერირების წესის განსაზღვრა
            c.CustomSchemaIds(x => x.FullName?[(x.FullName.LastIndexOf('.') + 1)..].Replace('+', '.'));

            // ავტორიზაციის წესების განსაზღვრა
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
            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                    { securityScheme, Array.Empty<string>() }
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

            // დკომენტარების დაყენება Swagger JSON და UI–თვის.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }
}