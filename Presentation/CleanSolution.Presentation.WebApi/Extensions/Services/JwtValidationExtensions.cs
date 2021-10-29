using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Workabroad.Presentation.WebApi.Extensions.Services
{
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

                        ClockSkew = TimeSpan.Zero, // ანულებს ტოკენის სიცოცხლის ხანგრძლივობას. დეფოლტად არის 5 წუთი
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
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

                // მისი გამოყენება მოხდება [Authorize(Policy = "EditUsersPolicy")] ატრიბუტით
                options.AddPolicy("Agent", policy => policy.RequireClaim("roles", "Agent"));
                options.AddPolicy("AllowedPeople", policy => policy.RequireClaim("id", "1", "2", "3", "4"));

                options.AddPolicy("EditPolicy", policy =>
                {
                    policy.RequireAssertion(con => con.User.HasClaim(x => x.Type == "resources" && x.Value == "user.edit"));
                });
                options.AddPolicy("DeletePolicy", policy =>
                {
                    policy.RequireAssertion(con => con.User.HasClaim(x => x.Type == "resources" && x.Value == "user.delete"));
                });
                options.AddPolicy("ViewPolicy", policy =>
                {
                    policy.RequireAssertion(con => con.User.HasClaim(x => x.Type == "resources" && x.Value == "user.view"));
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
            var signinCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            var jwt = new JwtSecurityToken
                (
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    issuer: configuration["JwtSettings:Issuer"],
                    audience: configuration["JwtSettings:Audience"],
                    signingCredentials: signinCredentials
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
