using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Columns.DeleteColumn;

public static class DeleteColumnEndpoint
{
    public static void MapDeleteColumnEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/boards/{boardId}/columns/{columnId}", async (
            Guid boardId,
            Guid columnId,
            IValidator<DeleteColumnCommand> validator,
            DeleteColumnHandler handler
        ) =>
        {
            var command = new DeleteColumnCommand
            {
                ColumnId = columnId
            };
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid column data.");
                return Results.BadRequest(response);
            }

            await handler.HandleAsync(command);

            return Results.NoContent();
        })
        .WithName("DeleteColumn")
        .WithTags("Columns");
    }
}