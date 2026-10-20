using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.WebApi.Extensions.Services;

/// <summary>
/// Global action filter: validates every action argument that has an IValidator&lt;T&gt; registered
/// (the nested Validator classes of the use cases) and throws ApiValidationException on failure,
/// which ExceptionHandler turns into a 400 ValidationProblemDetails.
/// </summary>
public sealed class FluentValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (serviceProvider.GetService(validatorType) is not IValidator validator) continue;

            var validationContext = new ValidationContext<object>(argument);
            var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!validationResult.IsValid)
                throw new ApiValidationException(validationResult.ToDictionary());
        }

        await next();
    }
}
