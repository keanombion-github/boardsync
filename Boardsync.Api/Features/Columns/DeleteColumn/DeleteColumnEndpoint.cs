using Boardsync.Api.Common.Models;
using Boardsync.Api.Common.Auth;
using FluentValidation;
using System.Security.Claims;

namespace Boardsync.Api.Features.Columns.DeleteColumn;

public static class DeleteColumnEndpoint
{
    public static void MapDeleteColumnEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/boards/{boardId}/columns/{columnId}", async (
            Guid boardId,
            Guid columnId,
            ClaimsPrincipal principal,
            IValidator<DeleteColumnCommand> validator,
            DeleteColumnHandler handler
        ) =>
        {
            var command = new DeleteColumnCommand
            {
                BoardId = boardId,
                ColumnId = columnId,
                UserId = principal.GetRequiredUserId()
            };
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid column data.");
                return Results.BadRequest(response);
            }

            var deleted = await handler.HandleAsync(command);

            if (!deleted)
            {
                return Results.NotFound(
                    ApiResponse<object>.Fail("NOT_FOUND", "Column not found on this board."));
            }

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteColumn")
        .WithTags("Columns");
    }
}
