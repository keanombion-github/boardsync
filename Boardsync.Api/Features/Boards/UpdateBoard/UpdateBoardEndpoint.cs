using System.Security.Claims;
using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Boards.UpdateBoard;

public sealed record UpdateBoardBody(string Name);

public static class UpdateBoardEndpoint
{
    public static void MapUpdateBoardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/boards/{id:guid}", async (
            Guid id,
            UpdateBoardBody body,
            ClaimsPrincipal principal,
            IValidator<UpdateBoardCommand> validator,
            UpdateBoardHandler handler) =>
        {
            var command = new UpdateBoardCommand(
                id,
                principal.GetRequiredUserId(),
                body.Name);
            var validation = await validator.ValidateAsync(command);

            if (!validation.IsValid)
            {
                return Results.BadRequest(ApiResponse<object>.Fail(
                    "VALIDATION_ERROR",
                    "Invalid board data.",
                    validation.Errors.Select(error => error.ErrorMessage)));
            }

            if (!await handler.HandleAsync(command))
            {
                return Results.NotFound(ApiResponse<object>.Fail(
                    "NOT_FOUND",
                    "Board not found."));
            }

            return Results.Ok(ApiResponse<object>.Ok(new
            {
                id,
                name = command.Name.Trim()
            }));
        })
        .RequireAuthorization()
        .WithName("UpdateBoard")
        .WithTags("Boards");
    }
}
