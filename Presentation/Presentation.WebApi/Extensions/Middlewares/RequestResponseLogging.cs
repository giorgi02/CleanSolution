namespace Presentation.WebApi.Extensions.Middlewares;

public class RequestResponseLogging(RequestDelegate next, ILogger<RequestResponseLogging> logger)
{
    public async Task Invoke(HttpContext context)
    {
        await FormatRequest(context.Request);

        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();

        context.Response.Body = responseBody;

        await next(context);

        await FormatResponse(context.Response);
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private async Task FormatRequest(HttpRequest request)
    {
        //request.EnableRewind();
        HttpRequestRewindExtensions.EnableBuffering(request);
        var body = request.Body;

        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        _ = await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        body.Seek(0, SeekOrigin.Begin);
        request.Body = body;

        logger.LogInformation("request scheme={@scheme} host={@Host} path={@Path} queryString={@QueryString} body={@Body}",
             request.Scheme, request.Host, request.Path, request.QueryString, bodyAsText);
    }

    private async Task FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        logger.LogInformation("response body={@Body}", bodyAsText);
    }
}