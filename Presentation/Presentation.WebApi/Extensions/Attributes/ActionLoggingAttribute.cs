using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.WebApi.Extensions.Attributes;
public class ActionLoggingAttribute : ActionFilterAttribute
{
    private const string DurationKey = "DurationKey";
    private readonly ILogger<ActionLoggingAttribute> _logger;
    private readonly IActiveUserService _user;

    public ActionLoggingAttribute(ILogger<ActionLoggingAttribute> logger, IActiveUserService user)
    {
        _logger = logger;
        _user = user;
    }

    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is SkipActionLoggingAttribute))
        {
            var request = context.HttpContext.Request;
            context.HttpContext.Items[DurationKey] = DateTime.Now;


            _logger.LogInformation("*-> request= url: {@RequestUrl}, method: {@RequestMethod}, type: {@Type}, {@ActionArguments}, {@IpAddress}, {@Port}, {@Scheme}, {@Host}, {@Path}, {@UserId}",
                _user.RequestedUrl, _user.RequestedMethod, request.ContentType, context.ActionArguments, _user.IpAddress, _user.Port, _user.Scheme, _user.Host, _user.Path, _user.UserId);
        }

        return base.OnActionExecutionAsync(context, next);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (!context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is SkipActionLoggingAttribute))
        {
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;
            var body = context.Result switch
            {
                JsonResult jsonResult => jsonResult.Value,
                ObjectResult objectResult => objectResult.Value,
                _ => null,
            };

            var duration = (DateTime.Now - Convert.ToDateTime(context.HttpContext.Items[DurationKey])).TotalMilliseconds;

            // todo: დასამატებელია Type და Body ის სწორად დამუშავება
            _logger.LogInformation("<-* response= type: {@Type}, StatusCode: {@StatusCode}, {@Body}, {@Duration}", response.ContentType, response.StatusCode, body, duration);
        }

        base.OnActionExecuted(context);
    }
}