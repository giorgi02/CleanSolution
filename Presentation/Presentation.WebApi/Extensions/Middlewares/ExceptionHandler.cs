using Core.Application.Commons;
using Core.Application.Exceptions;
using System.Diagnostics;

namespace Presentation.WebApi.Extensions.Middlewares;
public class ExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger) =>
        (this._next, this._logger) = (next, logger);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
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
            case EntityValidationException e:
                status = (int)e.StatusCode;
                errors = new(e.Messages);
                _logger.LogWarning(e, "{@ex} {@TraceId}", nameof(EntityValidationException), traceId);
                break;
            case OperationCanceledException e:
                title = "Operation Is Canceled.";
                errors.Add(ConstantValues.ExceptionMessage, ["Operation Is Canceled."]);
                _logger.LogWarning(e, "{@ex} {@TraceId}", nameof(OperationCanceledException), traceId);
                break;
            case { } e:
                title = "Server Error.";
                status = StatusCodes.Status500InternalServerError;
                errors.Add(ConstantValues.ExceptionMessage, ["Internal Server Error."]);
                // todo: დავაკვირდე ამ მეთოდს (Demystify)
                _logger.LogError(e.Demystify(), "{@ex} {@TraceId}", nameof(Exception), traceId);
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