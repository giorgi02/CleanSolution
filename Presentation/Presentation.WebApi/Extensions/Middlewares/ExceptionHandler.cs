using Core.Shared.Exceptions;
using System.Diagnostics;

namespace Presentation.WebApi.Extensions.Middlewares;

public class ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        string traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        ProblemDetails problemDetails = null!;

        switch (exception)
        {
            case ApiValidationException e:
                problemDetails = new ValidationProblemDetails
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5",
                    Status = (int)e.StatusCode,
                    Title = "One or more validation errors occurred.",
                    Errors = e.Messages
                };
                logger.LogWarning(e, "{location} {traceId}", nameof(ApiValidationException), traceId);
                break;
            case OperationCanceledException e:
                problemDetails = new ProblemDetails
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231",
                    Status = StatusCodes.Status499ClientClosedRequest,
                    Title = "Operation Is Canceled.",
                    Detail = "Operation Is Canceled.",
                };
                logger.LogWarning(e, "{location} {traceId}", nameof(OperationCanceledException), traceId);
                break;
            case { } e:
                problemDetails = new ProblemDetails
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server Error.",
                    Detail = "Internal Server Error.",
                };
                logger.LogError(e, "{location} {traceId}", nameof(Exception), traceId);
                break;
        }

        problemDetails.Extensions.Add("traceId", traceId);
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}