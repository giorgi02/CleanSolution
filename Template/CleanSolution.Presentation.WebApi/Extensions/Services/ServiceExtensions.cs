using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace Workabroad.Presentation.Admin.Extensions.Services
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "AnyPolicy", builder => builder
                    .AllowAnyOrigin() // დაშვება ეძლევა მოთხოვნას ნებისმიერი წყაროდან
                    .AllowAnyMethod() // დაშვებას იძლევა HTTP ყველა მეთოდზე
                    .AllowAnyHeader()
                    .WithExposedHeaders("AccessToken", "PageIndex", "PageSize", "TotalPages", "TotalCount", "HasPreviousPage", "HasNextPage"));
            });
        }

        public static void ConfigureXml(this IServiceCollection services)
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
