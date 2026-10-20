using System.Diagnostics;

namespace Presentation.WebApi.Extensions.Middlewares;

/// <summary>
/// The single place where exceptions become HTTP responses (RFC 9110 problem details):
/// ApiValidationException (and subclasses) => ValidationProblemDetails with the exception's status code,
/// OperationCanceledException => 499, anything else => 500 without internal details.
/// </summary>
public sealed class ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
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
        ProblemDetails problemDetails;

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
            default:
                problemDetails = new ProblemDetails
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server Error.",
                    Detail = "Internal Server Error.",
                };
                logger.LogError(exception, "{location} {traceId}", nameof(Exception), traceId);
                break;
        }

        if (context.Response.HasStarted) return;

        problemDetails.Extensions.Add("traceId", traceId);
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), options: null, contentType: "application/problem+json");
    }
}
