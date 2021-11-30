using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.Exceptions;
using FluentValidation;
using System.Diagnostics;
using System.Net;

namespace CleanSolution.Presentation.WebApi.Extensions.Middlewares;
public class ExceptionHandler
{
    private readonly RequestDelegate next;
    private readonly ILogger logger;

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
        string titleText = "One or more validation errors occurred.";
        var statusCode = (int)HttpStatusCode.BadRequest;
        var traceId = Activity.Current?.Id ?? context?.TraceIdentifier;

        switch (exception)
        {
            case ApplicationBaseException e:
                logger.LogWarning(e, nameof(ApplicationBaseException));
                statusCode = (int)e.StatusCode;
                break;
            case ValidationException e:
                logger.LogWarning(e, nameof(ValidationException));
                statusCode = StatusCodes.Status422UnprocessableEntity;
                break;
            case OperationCanceledException e:
                logger.LogWarning(e, nameof(OperationCanceledException));
                exception = new Exception("Operation Is Canceled.");
                titleText = "Operation Is Canceled.";
                break;
            case Exception e:
                logger.LogError(e, e.Message);
                exception = new Exception("Internal Server Error.");
                titleText = "Server Error.";
                statusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }


        context!.Response.ContentType = "application/json";
        context!.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(
        JsonSerializer.Serialize(Response.Failure(
            titleText: titleText,
            statusCode: statusCode,
            traceId: traceId,
            exception: exception
        )));
    }
}