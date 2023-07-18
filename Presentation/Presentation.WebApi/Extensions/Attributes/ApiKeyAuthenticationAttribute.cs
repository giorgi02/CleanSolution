using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        _apiKey = configuration["ApiKey"] ?? throw new ArgumentNullException("ApiKey");


    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous) return;

        if (context.HttpContext.Request.Headers["x-api-key"] != _apiKey)
            context.Result = new JsonResult("ApiKey is invalid")
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
    }
}