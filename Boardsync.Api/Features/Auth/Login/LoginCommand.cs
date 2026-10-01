namespace Boardsync.Api.Features.Auth.Login;

public sealed class LoginCommand
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
