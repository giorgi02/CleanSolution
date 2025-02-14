using System.Diagnostics;

namespace Presentation.WebApi.Extensions.Middlewares;
public static class CorrelationMiddleware
{
    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (!Guid.TryParse(context.Request.Headers["x-correlation-id"], out var activityId))
                activityId = Guid.NewGuid();

            Trace.CorrelationManager.ActivityId = activityId;
            await next.Invoke();
        });
        return app;
    }
}
