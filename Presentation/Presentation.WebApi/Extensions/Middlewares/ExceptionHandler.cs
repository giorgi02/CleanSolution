using Core.Application.Commons;
using Core.Application.Exceptions;
using System.Diagnostics;
using System.Net;

namespace Presentation.WebApi.Extensions.Middlewares;
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
        var responce = ResponseFactory.CreateFailure(Activity.Current?.Id ?? context?.TraceIdentifier);

        switch (exception)
        {
            case EntityValidationException e:
                logger.LogWarning(e, "{@ex} {@TraceId}", nameof(EntityValidationException), responce.TraceId);
                responce.SetStatus(e.StatusCode).SetErrors(e.Errors);
                break;
            case ApplicationBaseException e:
                logger.LogWarning(e, "{@ex} {@TraceId}", nameof(ApplicationBaseException), responce.TraceId);
                responce.SetStatus(e.StatusCode).SetErrors(e.Message);
                break;
            case OperationCanceledException e:
                logger.LogWarning(e, "{@ex} {@TraceId}", nameof(OperationCanceledException), responce.TraceId);
                responce.SetTitle("Operation Is Canceled.").SetErrors("Operation Is Canceled.");
                break;
            case Exception:
                logger.LogError(exception, "{@ex} {@TraceId}", nameof(Exception), responce.TraceId);
                responce.SetTitle("Server Error.").SetStatus(HttpStatusCode.InternalServerError).SetErrors("Internal Server Error.");
                break;
        }


        context!.Response.ContentType = "application/json";
        context!.Response.StatusCode = responce.Status;

        await context.Response.WriteAsJsonAsync(responce);
    }
}