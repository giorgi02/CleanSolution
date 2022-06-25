using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.WebApi.Extensions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string _token = "80f38646-de65-445b-8c7e-97035472b6fa";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous) return;

            var token = Convert.ToString(context.HttpContext.Request.Headers["Authorization"]);

            await Task.Delay(0);

            if (token == null || token.Replace("Bearer ", "") != _token)
                context.Result = new JsonResult(null)
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
        }
    }
}
