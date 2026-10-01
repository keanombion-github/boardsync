namespace Boardsync.Api.Features.Auth.Register;

public enum RegisterResultCode
{
    Registered,
    EmailAlreadyExists
}

public sealed record RegisterResult(
    RegisterResultCode Code,
    Guid? UserId)
{
    public static RegisterResult Success(Guid userId)
    {
        return new RegisterResult(
            RegisterResultCode.Registered,
            userId);
    }

    public static RegisterResult EmailConflict()
    {
        return new RegisterResult(
            RegisterResultCode.EmailAlreadyExists,
            null);
    }
}