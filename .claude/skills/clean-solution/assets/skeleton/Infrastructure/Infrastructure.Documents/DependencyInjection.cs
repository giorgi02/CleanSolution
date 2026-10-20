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
        var minioSettings = configuration.GetSection("MinioConfigs").Get<MinioSettings>()
            ?? throw new InvalidOperationException("Configuration section 'MinioConfigs' is missing.");
        minioSettings.Validate();

        services.AddSingleton(minioSettings);
        services.AddScoped<IDocumentService, DocumentService>();

        services.AddMinio(options =>
        {
            options.Endpoint = minioSettings.MinioClient.Endpoint;
            options.AccessKey = minioSettings.MinioClient.AccessKey;
            options.SecretKey = minioSettings.MinioClient.SecretKey;
        });

        return services;
    }
}
