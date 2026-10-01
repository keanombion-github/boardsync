using Boardsync.Api.Common.Auth;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Boardsync.Api.Tests;

public sealed class AuthTokenServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateAccessToken_ContainsIdentityAndExpectedExpiration()
    {
        var service = CreateService();
        var user = new AuthUser(
            Guid.Parse("2f405702-e8e0-4ab5-8af1-aa725b929447"),
            "developer@example.com",
            "Board Developer");

        var result = service.CreateAccessToken(user);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal(Now.UtcDateTime.AddMinutes(15), result.ExpiresAtUtc);
        Assert.Equal("Boardsync.Api", token.Issuer);
        Assert.Contains("Boardsync.Web", token.Audiences);
        Assert.Equal(
            user.Id.ToString(),
            token.Claims.Single(claim =>
                claim.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(
            user.Email,
            token.Claims.Single(claim =>
                claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(
            user.DisplayName,
            token.Claims.Single(claim =>
                claim.Type == ClaimTypes.Name).Value);
    }

    [Fact]
    public void CreateRefreshToken_ReturnsUniqueRawTokensAndStableHashes()
    {
        var service = CreateService();

        var first = service.CreateRefreshToken();
        var second = service.CreateRefreshToken();

        Assert.NotEqual(first.Token, second.Token);
        Assert.NotEqual(first.TokenHash, second.TokenHash);
        Assert.Equal(64, first.TokenHash.Length);
        Assert.Equal(
            first.TokenHash,
            service.HashRefreshToken(first.Token));
        Assert.Equal(Now.UtcDateTime.AddDays(7), first.ExpiresAtUtc);
    }

    private static AuthTokenService CreateService()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "Boardsync.Api",
            Audience = "Boardsync.Web",
            SigningKey = new string('k', 64),
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        });

        return new AuthTokenService(options, new FixedTimeProvider(Now));
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
