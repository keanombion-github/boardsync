using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Columns.CreateColumn;

public static class CreateColumnEndpoint
{
    public static void MapCreateColumnEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/boards/{boardId}/columns", async (
            Guid boardId,
            CreateColumnBody body,
            IValidator<CreateColumnCommand> validator,
            CreateColumnHandler handler
        ) =>
        {
            var command = new CreateColumnCommand
            {
                Name = body.Name,
                BoardId = boardId
            };
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid column data.");
                return Results.BadRequest(response);
            }

            var columnId = await handler.HandleAsync(command);

            return Results.Created($"/api/boards/{command.BoardId}/columns/{columnId}", ApiResponse<object>.Ok(new { Id = columnId }));
        })
        .WithName("CreateColumn")
        .WithTags("Columns");
    }
}