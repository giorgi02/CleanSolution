using Core.Application.Interfaces.Services;
using Infrastructure.Documents.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio.AspNetCore;

namespace Infrastructure.Documents;
public static class ServiceExtensions
{
    public static void AddDocumentsLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDocumentService, DocumentService>();


        services.AddMinio(options =>
        {
            options.Endpoint = configuration["Minio:MinioClient:endpoint"] ?? throw new ArgumentNullException("Minio:MinioClient:endpoint");
            options.AccessKey = configuration["Minio:MinioClient:accessKey"] ?? throw new ArgumentNullException("Minio:MinioClient:accessKey");
            options.SecretKey = configuration["Minio:MinioClient:secretKey"] ?? throw new ArgumentNullException("Minio:MinioClient:secretKey");

            //options.ConfigureClient(client =>
            //{
            //    client.WithSSL();
            //    //client.WithSSL().WithTimeout(1000);
            //});
        });
    }
}
