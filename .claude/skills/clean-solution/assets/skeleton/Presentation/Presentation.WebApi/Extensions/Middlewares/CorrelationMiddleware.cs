using System.Diagnostics;

namespace Presentation.WebApi.Extensions.Middlewares;

public static class CorrelationMiddleware
{
    /// <summary>
    /// Takes the "x-correlation-id" header (or creates a new id) and makes it the ambient correlation id,
    /// which CorrelationDelegatingHandler forwards to downstream services.
    /// </summary>
    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (!Guid.TryParse(context.Request.Headers["x-correlation-id"], out var activityId))
                activityId = Guid.NewGuid();

            Trace.CorrelationManager.ActivityId = activityId;
            context.Response.Headers["x-correlation-id"] = activityId.ToString();

            await next.Invoke();
        });
        return app;
    }
}
