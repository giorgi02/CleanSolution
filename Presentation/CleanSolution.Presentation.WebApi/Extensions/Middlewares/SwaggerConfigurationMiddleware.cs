namespace CleanSolution.Presentation.WebApi.Extensions.Middlewares;
public static class SwaggerConfigurationMiddleware
{
    // middleware
    public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app, params string[] options)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.InjectStylesheet("/SwaggerDark.css"); // შავი ფონის დაყენება

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