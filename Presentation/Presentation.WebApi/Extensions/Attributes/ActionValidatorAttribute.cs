using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.WebApi.Extensions.Attributes;
/// <summary>
/// ეს ატრიბუტი ეძებს FluentValidation ის კლასებიდან რექვესტის შესაბამისს და იძახებს მის შემოწმების მეთოდს
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ActionValidatorAttribute<TRequest> : ActionFilterAttribute where TRequest : new()
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var model = context.ActionArguments.Values.OfType<TRequest>().FirstOrDefault();
        if (model == null)
        {
            context.Result = new ObjectResult("Model type not correct") { StatusCode = 500 };
            return;
        }

        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        if (validator == null)
        {
            context.Result = new ObjectResult("No validator is found") { StatusCode = 500 };
            return;
        }

        var validationResult = await validator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            context.Result = new BadRequestObjectResult(validationResult);
            return;
        }

        await base.OnActionExecutionAsync(context, next);
    }
}