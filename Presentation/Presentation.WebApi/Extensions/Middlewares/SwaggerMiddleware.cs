namespace Presentation.WebApi.Extensions.Middlewares;

public static class SwaggerMiddleware
{
    public static IApplicationBuilder UseCustomSwaggerUI(this IApplicationBuilder app)
    {
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "v1");
        });

        return app;
    }
}
