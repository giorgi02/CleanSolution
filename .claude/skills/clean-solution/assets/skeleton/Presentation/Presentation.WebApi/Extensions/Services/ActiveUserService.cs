using Core.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Presentation.WebApi.Extensions.Services;

/// <summary>
/// Current user and request information, read from HttpContext.
/// The first constructor is used by DI, the second one allows creating it by hand.
/// </summary>
public sealed class ActiveUserService : IActiveUserService
{
    public ActiveUserService(IHttpContextAccessor httpContextAccessor) : this(httpContextAccessor.HttpContext) { }
    public ActiveUserService(HttpContext? context)
    {
        if (context == null) return;

        this.UserId = FindingClaim(context, ClaimTypes.NameIdentifier);
        this.IpAddress = context.Request.Headers["x-forwarded-for"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        this.Port = context.Connection.RemotePort;

        this.Scheme = context.Request.Scheme;
        this.Host = Convert.ToString(context.Request.Host);
        this.Path = context.Request.Path;
        this.QueryString = Convert.ToString(context.Request.QueryString);
        this.RequestedMethod = context.Request.Method;
    }

    /// <summary>
    /// Prefers the authenticated principal. Falls back to reading the bearer token WITHOUT validating it,
    /// which is only acceptable when a gateway in front of the service already validated the token.
    /// </summary>
    private static string? FindingClaim(HttpContext context, string claim)
    {
        var idFromIdentity = context.User.FindFirstValue(claim);
        if (idFromIdentity != null) return idFromIdentity;

        var token = context.Request.Headers.Authorization.ToString();
        if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null;

        var handler = new JwtSecurityTokenHandler();
        var rawToken = token["Bearer ".Length..].Trim();
        if (!handler.CanReadToken(rawToken)) return null;

        return handler.ReadJwtToken(rawToken).Claims.FirstOrDefault(x => x.Type == claim)?.Value;
    }

    public string? UserId { get; }
    public string? IpAddress { get; }
    public int Port { get; }

    public string? Scheme { get; }
    public string? Host { get; }
    public string? Path { get; }
    public string? QueryString { get; }

    public string? RequestedUrl => this.Scheme is null ? null : $"{this.Scheme}://{this.Host}{this.Path}{this.QueryString}";
    public string? RequestedMethod { get; }
}
