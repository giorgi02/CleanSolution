using CleanSolution.Core.Application.Interfaces.Contracts;
using CleanSolution.Presentation.WebApi.Extensions.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workabroad.Presentation.WebApi.Extensions.Services;

namespace CleanSolution.Presentation.WebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddThisLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers().AddFluentValidation();

            // todo: მივაქციო ყურადღება, ზოგგან მუშაობს ზოგგან არა
            services.AddHttpContextAccessor(); // IHttpContextAccessor -ის ინექციისთვის
            services.AddScoped<IActiveUserService, ActiveUserService>();

            services.AddConfigureCors(configuration);
            services.AddConfigureHealthChecks(configuration);
            services.AddSwaggerServices("CleanSolution v1");
        }

        private static void AddConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "CorsPolicy", builder => builder
                    .AllowAnyOrigin() // დაშვება ეძლევა მოთხოვნას ნებისმიერი წყაროდან
                    .AllowAnyMethod() // დაშვებას იძლევა HTTP ყველა მეთოდზე
                    .AllowAnyHeader()
                    .WithExposedHeaders("AccessToken", "PageIndex", "PageSize", "TotalPages", "TotalCount", "HasPreviousPage", "HasNextPage"));
            });
        }

        private static void ConfigureXml(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                // Add XML Content Negotiation
                options.RespectBrowserAcceptHeader = true;
                options.InputFormatters.Add(new XmlSerializerInputFormatter(options));
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
            });
        }
    }
}
