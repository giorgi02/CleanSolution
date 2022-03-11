using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.Exceptions;
using System.Diagnostics;
using System.Net;

namespace CleanSolution.Presentation.WebApi.Extensions.Middlewares;
public class ExceptionHandler
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandler> logger;

    public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger) =>
        (this.next, this.logger) = (next, logger);

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
        var traceId = Activity.Current?.Id ?? context?.TraceIdentifier;
        var titleText = "One or more validation errors occurred.";
        var statusCode = (int)HttpStatusCode.BadRequest;
        var errors = new Dictionary<string, string[]>(1);

        switch (exception)
        {
            case EntityValidationException e:
                logger.LogWarning(e, nameof(EntityValidationException));
                statusCode = (int)e.StatusCode;
                errors = new Dictionary<string, string[]>(e.Errors);
                break;
            case ApplicationBaseException e:
                logger.LogWarning(e, nameof(ApplicationBaseException));
                statusCode = (int)e.StatusCode;
                errors.TryAdd("messages", new string[] { e.Message });
                break;
            case OperationCanceledException:
                logger.LogWarning(exception, nameof(OperationCanceledException));
                titleText = "Operation Is Canceled.";
                errors.TryAdd("messages", new string[] { "Operation Is Canceled." });
                break;
            case Exception:
                logger.LogError(exception, nameof(Exception));
                titleText = "Server Error.";
                statusCode = (int)HttpStatusCode.InternalServerError;
                errors.TryAdd("messages", new string[] { "Internal Server Error." });
                break;
        }


        context!.Response.ContentType = "application/json";
        context!.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(
        JsonSerializer.Serialize(Response.Failure(
            titleText: titleText,
            statusCode: statusCode,
            traceId: traceId,
            errors: errors
        )));
    }
}