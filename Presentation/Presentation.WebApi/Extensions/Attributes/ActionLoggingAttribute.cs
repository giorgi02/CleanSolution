using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.WebApi.Extensions.Attributes;
public sealed class ActionLoggingAttribute : ActionFilterAttribute
{
    private readonly IActiveUserService _user;
    private readonly ILogger<ActionLoggingAttribute> _logger;
    private const string DurationStamp = "DurationStamp";

    public ActionLoggingAttribute(IActiveUserService user, ILogger<ActionLoggingAttribute> logger)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // => 1, 3
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is SkipActionLoggingAttribute))
        {
            context.HttpContext.Items[DurationStamp] = DateTime.Now;

            context.ActionArguments.TryGetValue("request", out var body);

            var type = body?.GetType().FullName;

            _logger.LogInformation("*-> request= url: {@RequestedUrl}, method: {@RequestedMethod}, type: {@Type}, {@Body}, {@IpAddress}, {@Port}, {@Scheme}, {@Host}, {@Path}, {@UserId}",
                _user.RequestedUrl, _user.RequestedMethod, type, body, _user.IpAddress, _user.Port, _user.Scheme, _user.Host, _user.Path, _user.UserId);
        }

        return base.OnActionExecutionAsync(context, next);
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
    public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is SkipActionLoggingAttribute))
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

            _logger.LogInformation("response= type: {@Type}, statusCode: {@StatusCode}, duration: {@Duration}, {@Body}",
                type, response.StatusCode, duration, body);
        }

        return base.OnResultExecutionAsync(context, next);
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


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipActionLoggingAttribute : ActionFilterAttribute { }