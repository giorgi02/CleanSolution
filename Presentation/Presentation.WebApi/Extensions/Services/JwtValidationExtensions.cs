using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Presentation.WebApi.Extensions.Services;
public static class JwtValidationExtensions
{
    /// <summary>
    /// ავთენთიფიკაციის პარამეტრების დამატება (ტოკენის ვალიდურობის შემოწმება)
    /// </summary>
    public static void AddJwtAuthenticationConfigs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateActor = false,

                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,

                    //ClockSkew = TimeSpan.Zero, // ანულებს ტოკენის სიცოცხლის ხანგრძლივობას. დეფოლტად არის 5 წუთი
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]))
                };
            });
    }

    /// <summary>
    /// ავტორიზაციის პარამეტრების დამატება (მეთოდებზე დაშვების შემოწმება)
    /// </summary>
    public static void AddJwtAuthorizationConfigs(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

            // მისი გამოყენება მოხდება [Authorize(Policy = "EditUsersPolicy")] ატრიბუტით
            options.AddPolicy("Agent", policy => policy.RequireClaim("roles", "Agent"));

            options.AddPolicy("CreateUser", policy => policy.RequireClaim("resources", "create:user"));
            options.AddPolicy("UpdateUser", policy => policy.RequireClaim("resources", "update:user"));
            options.AddPolicy("DeleteUser", policy => policy.RequireClaim("resources", "delete:user"));

            options.AddPolicy("AllowedPeople", policy =>
            {
                policy.RequireClaim("id", "1", "2", "3", "4");
            });

            options.AddPolicy("TestPolicy", policy =>
            {
                policy.RequireAssertion(con => con.User.HasClaim(x => x.Type == "resources" && x.Value == "create:user"));
            });
        });
    }

    /// <summary>
    /// ტოკენის გენერირება
    /// </summary>
    public static string GenerateJwtToken(
        IConfiguration configuration,
        string userId,
        string userName,
        string[] roles,
        string[] resources)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("UserName", userName)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var resource in resources)
            claims.Add(new Claim("resources", resource));


        // ქმნის JWT ხელმოწერას
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));
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