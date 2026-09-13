using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Columns.ReorderColumn;

public static class ReorderColumnEndpoint {
    public static void MapReorderColumnEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/boards/{boardId}/columns/reorder", async (
            Guid boardId,
            ReorderColumnBody body,
            IValidator<ReorderColumnCommand> validator,
            ReorderColumnHandler handler
        ) => {
            
            var command = new ReorderColumnCommand
            {
                BoardId = boardId, 
                ColumnId = body.ColumnId, 
                BeforePosition = body.BeforePosition, 
                AfterPosition = body.AfterPosition
            };

            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid column data.");
                return Results.BadRequest(response);
            }

            var success = await handler.HandleAsync(command);
            if (!success)
            {
                return Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Column not found."));
            }
            return Results.Ok(ApiResponse<object>.Ok(new { boardId }));
        })
        .WithName("ReorderColumn")
        .WithTags("Columns");
    }
}