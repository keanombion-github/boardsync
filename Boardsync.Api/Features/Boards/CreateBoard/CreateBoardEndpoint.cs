using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Boards.CreateBoard;

/// <summary>
/// Defines the Minimal API endpoint for creating a board.
/// </summary>
public static class CreateBoardEndpoint
{
    public static void MapCreateBoardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/boards", async (
            CreateBoardCommand command, 
            IValidator<CreateBoardCommand> validator, 
            CreateBoardHandler handler) =>
        {
            // 1. Validate
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid board data.");
                // We could attach 'errors' to the ApiError Details property if we extend ApiError to have a setter
                // For simplicity, let's just return a 400 Bad Request
                return Results.BadRequest(response);
            }

            // 2. Execute Handler
            var boardId = await handler.HandleAsync(command);

            // 3. Return Standard Response
            return Results.Created($"/api/boards/{boardId}", ApiResponse<object>.Ok(new { Id = boardId }));
        })
        .WithName("CreateBoard")
        .WithTags("Boards");
    }
}
