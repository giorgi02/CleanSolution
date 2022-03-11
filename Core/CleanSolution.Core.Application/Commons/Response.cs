namespace CleanSolution.Core.Application.Commons;
public static class Response
{
    private const string type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

    public static object Success(bool isSuccess, params string[] messages) => new
    {
        isSuccess,
        messages
    };

    public static object Failure(string titleText, int statusCode, string? traceId, IDictionary<string, string[]> errors) => new
    {
        type,
        title = titleText,
        status = statusCode,
        traceId,
        errors
    };
}