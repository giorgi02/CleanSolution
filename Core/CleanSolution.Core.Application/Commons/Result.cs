using System;

namespace CleanSolution.Core.Application.Commons
{
    public static class Result
    {
        public static object Success(bool isSuccess, params string[] messages) => new
        {
            isSuccess = isSuccess,
            messages = messages
        };

        public static object Failure(string titleText, int statusCode, string traceId, params string[] messages) => new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = titleText,
            status = statusCode,
            traceId = traceId,
            errors = new
            {
                messages = messages
            }
        };

        public static object Failure(string titleText, int statusCode, string traceId, Exception exception) => new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = titleText,
            status = statusCode,
            traceId = traceId,
            errors = new
            {
                messages = new string[] { exception.InnerException?.InnerException?.Message ?? exception.InnerException?.Message ?? exception.Message }
            }
        };
    }
}
