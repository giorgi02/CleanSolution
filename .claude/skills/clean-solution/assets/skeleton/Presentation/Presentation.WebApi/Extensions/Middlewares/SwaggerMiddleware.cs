namespace Presentation.WebApi.Extensions.Middlewares;

public static class SwaggerMiddleware
{
    /// <summary>
    /// Swagger UI on top of the built-in OpenAPI document (/openapi/v1.json).
    /// </summary>
    public static IApplicationBuilder UseCustomSwaggerUI(this IApplicationBuilder app)
    {
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "v1");
        });

        return app;
    }
}
