using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.WebApi.Extensions.Attributes;
/// <summary>
/// attribute რომელიც შეიძლება გამოვიყენოთ შიდა სერვისებს შორის, მარტივი ავთენთიფიკაციისთვის
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _apiKey;
    public ApiKeyAuthenticationAttribute(IConfiguration configuration) =>
        _apiKey = configuration.GetString("ApiKey");


    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous) return Task.CompletedTask;

        if (context.HttpContext.Request.Headers["x-api-key"] != _apiKey)
            context.Result = new JsonResult("ApiKey is invalid")
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        return Task.CompletedTask;
    }
}