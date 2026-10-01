using Boardsync.Api.Common.Database;
using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Boardsync.Api.Common.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to keep Program.cs clean.
/// Each method groups related service registrations together.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the database connection factory as a singleton.
    /// Singleton is correct here because the factory only holds the connection string —
    /// it doesn't hold an open connection. Each call to CreateConnection() creates a new one.
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));
        return services;
    }

    /// <summary>
    /// Registers all FluentValidation validators found in this assembly.
    /// This scans for any class that implements AbstractValidator<T> and registers it in DI.
    /// </summary>
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }

    public static IServiceCollection AddBoardsyncAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var signingKey = jwtSection["SigningKey"];

        if (string.IsNullOrWhiteSpace(signingKey)
            || Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be configured with at least 32 bytes.");
        }

        services.AddOptions<JwtOptions>()
            .Bind(jwtSection)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer),
                "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience),
                "Jwt:Audience is required.")
            .Validate(options => options.AccessTokenMinutes is >= 1 and <= 60,
                "Jwt:AccessTokenMinutes must be between 1 and 60.")
            .Validate(options => options.RefreshTokenDays is >= 1 and <= 30,
                "Jwt:RefreshTokenDays must be between 1 and 30.")
            .ValidateOnStart();

        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is required.");
        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is required.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(signingKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        await context.Response.WriteAsJsonAsync(
                            ApiResponse<object>.Fail(
                                "UNAUTHORIZED",
                                "Authentication is required."));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status403Forbidden;

                        await context.Response.WriteAsJsonAsync(
                            ApiResponse<object>.Fail(
                                "FORBIDDEN",
                                "You do not have permission to perform this action."));
                    }
                };
            });

        services.AddAuthorization();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IAuthTokenService, AuthTokenService>();
        services.AddSingleton<RefreshTokenCookieService>();

        return services;
    }
}
