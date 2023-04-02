namespace Presentation.WebApi.Extensions.Middlewares;
/// <summary>
/// middleware რომელიც შეიძლება გამოვიყენოთ შიდა სერვისებს შორის, მარტივი ავთენთიფიკაციისთვის
/// </summary>
public sealed class ApiKeyAuthentication
{
    private readonly RequestDelegate _next;
    private readonly string _key;

    public ApiKeyAuthentication(RequestDelegate next, IConfiguration configuration)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _key = configuration["ApiKey"] ?? throw new ArgumentNullException("ApiKey");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers["x-api-key"] == _key)
            await _next(context);
        else
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("ApiKey is invalid");
        }
    }
}