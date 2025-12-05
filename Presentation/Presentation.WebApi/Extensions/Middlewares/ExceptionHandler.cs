using Core.Shared;
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

        var problemDetails = new HttpValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
        };
        problemDetails.Extensions.Add("traceId", Activity.Current?.Id);

        switch (exception)
        {
            case ApiValidationException e:
                problemDetails.Status = (int)e.StatusCode;
                problemDetails.Errors = e.Messages;
                logger.LogWarning(e, "{@ex} {@TraceId}", nameof(ApiValidationException), traceId);
                break;
            case OperationCanceledException e:
                problemDetails.Title = "Operation Is Canceled.";
                problemDetails.Errors.Add(ConstantValues.ExceptionMessage, ["Operation Is Canceled."]);
                logger.LogWarning(e, "{@ex} {@TraceId}", nameof(OperationCanceledException), traceId);
                break;
            case { } e:
                problemDetails.Title = "Server Error.";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Errors.Add(ConstantValues.ExceptionMessage, ["Internal Server Error."]);
                logger.LogError(e, "{@ex} {@TraceId}", nameof(Exception), traceId);
                break;
        }

        context.Response.StatusCode = problemDetails.Status.Value;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}