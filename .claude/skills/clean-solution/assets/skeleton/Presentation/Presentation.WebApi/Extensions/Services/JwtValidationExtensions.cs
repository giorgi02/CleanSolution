using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Presentation.WebApi.Extensions.Services;

/// <summary>
/// Optional JWT support. Enable it in AddStartup by calling AddJwtAuthenticationConfigs + AddJwtAuthorizationConfigs.
/// Settings: "JwtSettings:Issuer", "JwtSettings:Audience", "JwtSettings:Key" (keep the key in user-secrets / environment).
/// </summary>
public static class JwtValidationExtensions
{
    /// <summary>
    /// Authentication: how a bearer token is validated.
    /// </summary>
    public static void AddJwtAuthenticationConfigs(this IServiceCollection services, IConfiguration configuration) =>
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateActor = false,

                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,

                    //ClockSkew = TimeSpan.Zero, // default tolerance is 5 minutes
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetString("JwtSettings:Key")))
                };
            });


    /// <summary>
    /// Authorization: every endpoint requires an authenticated user unless it is marked [AllowAnonymous].
    /// Named policies are used as [Authorize(Policy = "DeletePolicy")].
    /// </summary>
    public static void AddJwtAuthorizationConfigs(this IServiceCollection services) =>
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("DeletePolicy", policy => policy.RequireClaim("resources", "delete:user"));
        });


    /// <summary>
    /// Issues a token (for an auth endpoint or for tests).
    /// </summary>
    public static string GenerateJwtToken(
        IConfiguration configuration,
        string userId,
        string userName,
        string[] roles)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("UserName", userName)
        ];

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetString("JwtSettings:Key")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken
            (
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
