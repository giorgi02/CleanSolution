namespace Presentation.WebApi.Extensions.Services;

public static class OpenApiService
{
    public static IServiceCollection AddOpenApiInfo(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "CleanSolution.Presentation.WebApi";
                document.Info.Version = "v1.0";
                document.Info.Description = "ASP.NET Core Web API - CleanSolution";
                return Task.CompletedTask;
            });
        });

        return services;
    }
}
