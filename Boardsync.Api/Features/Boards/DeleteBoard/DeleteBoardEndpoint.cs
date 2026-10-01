using System.Security.Claims;
using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Boards.DeleteBoard;

public static class DeleteBoardEndpoint
{
    public static void MapDeleteBoardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/boards/{id:guid}", async (
            Guid id,
            ClaimsPrincipal principal,
            IValidator<DeleteBoardCommand> validator,
            DeleteBoardHandler handler) =>
        {
            var command = new DeleteBoardCommand(
                id,
                principal.GetRequiredUserId());
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

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteBoard")
        .WithTags("Boards");
    }
}
