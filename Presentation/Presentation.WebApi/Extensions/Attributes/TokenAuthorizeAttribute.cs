using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Presentation.WebApi.Extensions.Attributes;
[Obsolete("შეიძლება გამოყენება მხოლოდ დროებით")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TokenAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string _token = "b8f6f5c3-7da8-4176-8553-076567b4b6b7";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous) return;

        var token = Convert.ToString(context.HttpContext.Request.Headers[HeaderNames.Authorization]);

        if (token == null || token.Replace("Bearer ", "") != _token)
            context.Result = new JsonResult("Token is invalid")
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
    }
}