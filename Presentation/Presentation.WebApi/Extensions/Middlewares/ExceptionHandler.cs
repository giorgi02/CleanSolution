using Core.Application.Commons;
using Core.Application.Exceptions;
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
        string title = "One or more validation errors occurred.";
        int status = StatusCodes.Status400BadRequest;
        string traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var errors = new Dictionary<string, string[]>(1);

        switch (exception)
        {
            case ApiValidationException e:
                status = (int)e.StatusCode;
                errors = new(e.Messages);
                logger.LogWarning(e, "{@ex} {@TraceId}", nameof(ApiValidationException), traceId);
                break;
            case OperationCanceledException e:
                title = "Operation Is Canceled.";
                errors.Add(ConstantValues.ExceptionMessage, ["Operation Is Canceled."]);
                logger.LogWarning(e, "{@ex} {@TraceId}", nameof(OperationCanceledException), traceId);
                break;
            case { } e:
                title = "Server Error.";
                status = StatusCodes.Status500InternalServerError;
                errors.Add(ConstantValues.ExceptionMessage, ["Internal Server Error."]);
                logger.LogError(e, "{@ex} {@TraceId}", nameof(Exception), traceId);
                break;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var response = new HttpValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231",
            Title = title,
            Status = status,
        };
        response.Extensions.Add("traceId", traceId);

        await context.Response.WriteAsJsonAsync(response);
    }
}