using System.Text;
using BudgetFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BudgetFlow.API.Extensions;

/// <summary>
/// Configures JWT bearer authentication and authorization policies for the API.
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    public const string AuthenticatedUserPolicy = "AuthenticatedUser";

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(jwtOptions.Key))
        {
            throw new InvalidOperationException(
                "JWT signing key ('Jwt:Key') is not configured. Set it via 'dotnet user-secrets set \"Jwt:Key\" \"<value>\"' for Development, or via environment variables / a secret store in other environments.");
        }

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthenticatedUserPolicy, policy => policy.RequireAuthenticatedUser());

        return services;
    }
}
