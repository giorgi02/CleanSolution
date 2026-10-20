using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace Presentation.WebApi.Extensions.Attributes;

/// <summary>
/// Put on an action/controller whose request body must not be logged (passwords, files, large payloads).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipRequestLoggingAttribute : ActionFilterAttribute { }

/// <summary>
/// Put on an action/controller whose response body must not be logged (lists, files, sensitive data).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipResponseLoggingAttribute : ActionFilterAttribute { }

/// <summary>
/// Global filter: logs every action's request (before it runs) and response (with duration).
/// </summary>
public sealed class ActionLoggingAttribute(ILogger<ActionLoggingAttribute> logger) : ActionFilterAttribute
{
    private const string StartTimestamp = "ActionLogging.StartTimestamp";

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        context.HttpContext.Items[StartTimestamp] = Stopwatch.GetTimestamp();

        if (!HasFilter<SkipRequestLoggingAttribute>(context))
        {
            context.ActionArguments.TryGetValue("request", out var body);

            var type = body?.GetType().FullName;
            var user = context.HttpContext.RequestServices.GetRequiredService<IActiveUserService>();

            logger.LogInformation("*-> request= url: {@RequestedUrl}, method: {@RequestedMethod}, type: {@Type}, {@Body}, {@IpAddress}, {@Port}, {@Scheme}, {@Host}, {@Path}, {@UserId}",
                user.RequestedUrl, user.RequestedMethod, type, body, user.IpAddress, user.Port, user.Scheme, user.Host, user.Path, user.UserId);
        }

        await next();
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        if (HasFilter<SkipResponseLoggingAttribute>(context)) return;

        var body = context.Result switch
        {
            JsonResult jsonResult => jsonResult.Value,
            ObjectResult objectResult => objectResult.Value,
            _ => null,
        };

        var type = body?.GetType().FullName;

        var duration = context.HttpContext.Items[StartTimestamp] is long start
            ? Stopwatch.GetElapsedTime(start).TotalMilliseconds
            : (double?)null;

        logger.LogInformation("<-* response= type: {@Type}, statusCode: {@StatusCode}, duration: {@Duration}, {@Body}",
            type, context.HttpContext.Response.StatusCode, duration, body);
    }

    private static bool HasFilter<TFilter>(FilterContext context) =>
        context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is TFilter);
}
