using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Workabroad.Presentation.WebApi.Extensions.Middlewares
{
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
                    logger.LogWarning(exception, nameof(ApplicationBaseException));
                    statusCode = (int)e.StatusCode;
                    break;
                case ValidationException _:
                    logger.LogWarning(exception, nameof(ValidationException));
                    break;
                case OperationCanceledException _:
                    logger.LogError(exception, exception.Message);
                    titleText = "Operation Is Canceled.";
                    break;
                case Exception _:
                    logger.LogError(exception, exception.Message);
                    exception = new Exception("Internal Server Error.");
                    titleText = "Internal Server Error.";
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }


            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(
            JsonSerializer.Serialize(Result.Failure(
                titleText: titleText,
                statusCode: statusCode,
                traceId: traceId,
                exception: exception
            )));
        }
    }
}
