using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.WebApi.Extensions.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipRequestLoggingAttribute : ActionFilterAttribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipResponseLoggingAttribute : ActionFilterAttribute { }

public sealed class ActionLoggingAttribute(ILogger<ActionLoggingAttribute> logger) : ActionFilterAttribute
{
    private const string DurationStamp = "DurationStamp";

    // => 1, 3
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await base.OnActionExecutionAsync(context, next);

        if (!context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is SkipRequestLoggingAttribute))
        {
            context.HttpContext.Items[DurationStamp] = DateTime.Now;

            context.ActionArguments.TryGetValue("request", out var body);

            var type = body?.GetType().FullName;
            var user = context.HttpContext.RequestServices.GetRequiredService<IActiveUserService>();

            logger.LogInformation("*-> request= url: {@RequestedUrl}, method: {@RequestedMethod}, type: {@Type}, {@Body}, {@IpAddress}, {@Port}, {@Scheme}, {@Host}, {@Path}, {@UserId}",
                user.RequestedUrl, user.RequestedMethod, type, body, user.IpAddress, user.Port, user.Scheme, user.Host, user.Path, user.UserId);

            // todo: ეს მეთოდი დასამუშავებელია
            logger.LogInformation("headers: {@headers}", context.HttpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value));
        }
    }
    // => 2
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
    }
    // => 4
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        // P.S Exception ის დროს აქ შემოდის
        base.OnActionExecuted(context);
    }

    // <= 1, 4
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await base.OnResultExecutionAsync(context, next);

        if (!context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is SkipResponseLoggingAttribute))
        {
            var response = context.HttpContext.Response;

            var body = context.Result switch
            {
                JsonResult jsonResult => jsonResult.Value,
                ObjectResult objectResult => objectResult.Value,
                _ => null,
            };

            var type = body?.GetType().FullName;

            var duration = (DateTime.Now - Convert.ToDateTime(context.HttpContext.Items[DurationStamp])).TotalMilliseconds;

            logger.LogInformation("response= type: {@Type}, statusCode: {@StatusCode}, duration: {@Duration}, {@Body}",
                type, response.StatusCode, duration, body);
        }
    }
    // <= 2
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        base.OnResultExecuting(context);
    }
    // <= 3
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        base.OnResultExecuted(context);
    }
}