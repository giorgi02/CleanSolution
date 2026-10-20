using Core.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

namespace Presentation.WebApi.Extensions.Middlewares;

public static class ExceptionHandlerExtensions
{
    public static IServiceCollection AddExceptionHandler(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
            };
        });

        services.AddExceptionHandler<ExceptionHandler>();

        return services;
    }
}

public sealed class ExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        ProblemDetails problemDetails = null!;

        switch (exception)
        {
            case ApiValidationException e:
                problemDetails = new ValidationProblemDetails
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5",
                    Status = (int)e.StatusCode,
                    Title = "One or more validation errors occurred.",
                    Errors = e.Messages
                };
                logger.LogWarning(e, "{location} {traceId}", nameof(ApiValidationException), traceId);
                break;
            case OperationCanceledException e:
                problemDetails = new ProblemDetails
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc9110",
                    Status = StatusCodes.Status499ClientClosedRequest,
                    Title = "Operation Is Canceled.",
                    Detail = "Operation Is Canceled.",
                };
                logger.LogWarning(e, "{location} {traceId}", nameof(OperationCanceledException), traceId);
                break;
            case { } e:
                problemDetails = new ProblemDetails
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server Error.",
                    Detail = "Internal Server Error.",
                };
                logger.LogError(e, "{location} {traceId}", nameof(Exception), traceId);
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}
