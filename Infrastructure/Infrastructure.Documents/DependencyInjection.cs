using Core.Application.Interfaces.Services;
using Infrastructure.Documents.Implementations;
using Infrastructure.Documents.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio.AspNetCore;

namespace Infrastructure.Documents;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentsLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDocumentService, DocumentService>();

        var minioConfigs = configuration.GetSection("MinioConfigs").Get<MinioConfig>();
        var minioClient = minioConfigs?.MinioClient;

        services.AddMinio(options =>
        {
            options.Endpoint = minioClient?.Endpoint ?? throw new ArgumentNullException(nameof(minioClient.Endpoint));
            options.AccessKey = minioClient.AccessKey ?? throw new ArgumentNullException(nameof(minioClient.AccessKey));
            options.SecretKey = minioClient.SecretKey ?? throw new ArgumentNullException(nameof(minioClient.SecretKey));

            //options.ConfigureClient(client =>
            //{
            //    client.WithSSL();
            //    //client.WithSSL().WithTimeout(1000);
            //});
        });

        return services;
    }
}
