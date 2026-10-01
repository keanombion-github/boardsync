namespace Boardsync.Api.Common.Auth;

public sealed class RefreshTokenCookieService
{
    public const string CookieName = "boardsync_refresh";

    private readonly IWebHostEnvironment _environment;
    private readonly string _cookiePath;

    public RefreshTokenCookieService(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _cookiePath = configuration["RefreshCookiePath"] ?? "/api/auth";
    }

    public void Write(
        HttpResponse response,
        string token,
        DateTime expiresAtUtc)
    {
        response.Cookies.Append(
            CookieName,
            token,
            CreateOptions(expiresAtUtc));
    }

    public void Delete(HttpResponse response)
    {
        response.Cookies.Delete(
            CookieName,
            CreateOptions(DateTime.UnixEpoch));
    }

    private CookieOptions CreateOptions(DateTime expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = _cookiePath,
            Expires = new DateTimeOffset(expiresAtUtc)
        };
    }
}
