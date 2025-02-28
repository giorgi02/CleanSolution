using Core.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Presentation.WebApi.Extensions.Services;
public class ActiveUserService : IActiveUserService
{
    public ActiveUserService(IHttpContextAccessor httpContextAccessor) : this(httpContextAccessor.HttpContext) { }
    public ActiveUserService(HttpContext? context)
    {
        if (context == null) return;

        this.UserId = FindingClaim(context, ClaimTypes.NameIdentifier);
        this.IpAddress = context.Request.Headers["x-forwarded-for"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }

    private static string? FindingClaim(HttpContext context, string claim)
    {
        var idFromIdentity = context.User.FindFirstValue(claim);
        if (idFromIdentity != null) return idFromIdentity;


        var token = context.Request.Headers.Authorization.ToString();
        if (token.Length < "Bearer ".Length) return null;

        var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(token["Bearer ".Length..]);

        return jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == claim)?.Value;
    }

    public string? UserId { get; }
    public string? IpAddress { get; }
}