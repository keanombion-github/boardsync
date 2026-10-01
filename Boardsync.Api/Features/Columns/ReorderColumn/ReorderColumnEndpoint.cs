using Boardsync.Api.Common.Models;
using Boardsync.Api.Common.Auth;
using FluentValidation;
using System.Security.Claims;

namespace Boardsync.Api.Features.Columns.ReorderColumn;

public static class ReorderColumnEndpoint
{
    public static void MapReorderColumnEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/boards/{boardId}/columns/reorder", async (
            Guid boardId,
            ReorderColumnBody body,
            ClaimsPrincipal principal,
            IValidator<ReorderColumnCommand> validator,
            ReorderColumnHandler handler
        ) =>
        {
            var command = new ReorderColumnCommand
            {
                BoardId = boardId,
                UserId = principal.GetRequiredUserId(),
                ColumnId = body.ColumnId,
                BeforeColumnId = body.BeforeColumnId,
                AfterColumnId = body.AfterColumnId
            };

            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid column data.");
                return Results.BadRequest(response);
            }

            var result = await handler.HandleAsync(command);

            if (result == ReorderColumnResult.BoardNotFound)
                return Results.NotFound(
                    ApiResponse<object>.Fail("NOT_FOUND", "Board not found."));

            if (result == ReorderColumnResult.ColumnNotFound)
                return Results.NotFound(
                    ApiResponse<object>.Fail("NOT_FOUND", "Column not found on this board."));

            if (result == ReorderColumnResult.InvalidNeighbors)
                return Results.BadRequest(
                    ApiResponse<object>.Fail(
                        "INVALID_NEIGHBORS",
                        "Column neighbors must be adjacent columns on this board."));

            return Results.Ok(ApiResponse<object>.Ok(new { boardId }));
        })
        .RequireAuthorization()
        .WithName("ReorderColumn")
        .WithTags("Columns");
    }
}
