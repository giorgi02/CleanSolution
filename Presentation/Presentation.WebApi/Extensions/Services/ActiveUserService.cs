using Core.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Presentation.WebApi.Extensions.Services;
public class ActiveUserService : IActiveUserService
{
    /// <summary>
    /// იუზერის მონაცემების და მოთხოვნის ინფორმაციის ამოღება
    /// საჭიროა ორივე კონსტრუქტორი: პირველი IoC კონტეინერისთვის გამოიყენება, მეორე ხელიტ შექმნისთვის.
    /// </summary>
    public ActiveUserService(IHttpContextAccessor httpContextAccessor) : this(httpContextAccessor.HttpContext) { }
    public ActiveUserService(HttpContext? context)
    {
        if (context == null) return;

        this.UserId = Guid.TryParse(FindingClaim(context, ClaimTypes.NameIdentifier), out Guid result) ? result : null;
        this.IpAddress = context.Request.Headers["x-forwarded-for"].FirstOrDefault() ?? context.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
        this.Port = context.Connection?.RemotePort ?? 0;

        this.Scheme = context.Request.Scheme;
        this.Host = Convert.ToString(context.Request.Host);
        this.Path = context.Request.Path;
        this.QueryString = Convert.ToString(context.Request.QueryString);
        this.RequestedMethod = context.Request.Method;
    }

    private static string? FindingClaim(HttpContext context, string claim)
    {
        var idFromIdentity = context.User?.FindFirstValue(claim);
        if (idFromIdentity != null) return idFromIdentity;


        var token = context.Request.Headers.Authorization.ToString();
        if (token.Length < "Bearer ".Length) return null;

        var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(token["Bearer ".Length..]);

        return jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == claim)?.Value;
    }

    public Guid? UserId { get; }
    public string? IpAddress { get; }
    public int Port { get; }

    public string? Scheme { get; }
    public string? Host { get; }
    public string? Path { get; }
    private readonly string? QueryString;

    public string? RequestedUrl => $"{this.Scheme}://{this.Host}{this.Path}{this.QueryString}";
    public string? RequestedMethod { get; }
}