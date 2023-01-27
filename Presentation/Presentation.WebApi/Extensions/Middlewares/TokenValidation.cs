namespace Presentation.WebApi.Extensions.Middlewares;
public class TokenValidation
{
    private readonly RequestDelegate _next;
    private const string _token = "b8f6f5c3-7da8-4176-8553-076567b4b6b7";

    public TokenValidation(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var token = Convert.ToString(context.Request.Headers["Authorization"]) ?? "";

        if (token.Replace("Bearer ", "") == _token)
            await _next(context);

        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Token is invalid");
    }
}