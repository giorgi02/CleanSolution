using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;

namespace Presentation.WebApi.Extensions.Attributes;

/// <summary>
/// Simple API-key check for internal service-to-service calls (header "x-api-key", value from "ApiKey").
/// The filter needs DI, so apply it as: [TypeFilter(typeof(ApiKeyAuthenticationAttribute))]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiKeyAuthenticationAttribute(IConfiguration configuration) : Attribute, IAsyncAuthorizationFilter
{
    private readonly byte[] _apiKey = Encoding.UTF8.GetBytes(RequireKey(configuration.GetString("ApiKey")));

    private static string RequireKey(string key)
    {
        // an empty key would accept requests that send an empty header
        ArgumentException.ThrowIfNullOrWhiteSpace(key, "ApiKey");
        return key;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous) return Task.CompletedTask;

        var received = Encoding.UTF8.GetBytes(context.HttpContext.Request.Headers["x-api-key"].ToString());

        if (!CryptographicOperations.FixedTimeEquals(received, _apiKey))
            context.Result = new JsonResult("ApiKey is invalid")
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

        return Task.CompletedTask;
    }
}
