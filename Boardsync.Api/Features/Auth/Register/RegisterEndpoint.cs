using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Auth.Register;

public sealed record RegisterBody(
    string Email,
    string Password,
    string DisplayName);

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (
            RegisterBody body,
            IValidator<RegisterCommand> validator,
            RegisterHandler handler) =>
        {
            var command = new RegisterCommand
            {
                Email = body.Email,
                Password = body.Password,
                DisplayName = body.DisplayName
            };

            var validationResult =
                await validator.ValidateAsync(command);

            if (!validationResult.IsValid)
            {
                var details = validationResult.Errors
                    .Select(error => error.ErrorMessage);

                return Results.BadRequest(
                    ApiResponse<object>.Fail(
                        "VALIDATION_ERROR",
                        "Invalid registration data.",
                        details));
            }

            var result = await handler.HandleAsync(command);

            if (result.Code
                == RegisterResultCode.EmailAlreadyExists)
            {
                return Results.Conflict(
                    ApiResponse<object>.Fail(
                        "EMAIL_ALREADY_EXISTS",
                        "An account with this email already exists."));
            }

            if (result.Code != RegisterResultCode.Registered)
            {
                throw new InvalidOperationException(
                    $"Unexpected registration result: {result.Code}");
            }

            var userId = result.UserId
                ?? throw new InvalidOperationException(
                    "Successful registration did not return a user ID.");

            return Results.Json(
                ApiResponse<object>.Ok(new
                {
                    id = userId,
                    email = command.Email.Trim().ToLowerInvariant(),
                    displayName = command.DisplayName.Trim()
                }),
                statusCode: StatusCodes.Status201Created);
        })
        .AllowAnonymous()
        .RequireRateLimiting("auth-sensitive")
        .WithName("RegisterUser")
        .WithTags("Auth");
    }
}
