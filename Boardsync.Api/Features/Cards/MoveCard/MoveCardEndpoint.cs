using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Cards.MoveCard;

public static class MoveCardEndpoint {
    public static void MapMoveCardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/cards/{id}/move", async (
            Guid id,
            MoveCardBody body,
            IValidator<MoveCardCommand> validator,
            MoveCardHandler handler
        ) => {
            
            var command = new MoveCardCommand
            {
                Id = id, 
                ColumnId = body.ColumnId, 
                BeforePosition = body.BeforePosition, 
                AfterPosition = body.AfterPosition
            };

            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid card data.");
                return Results.BadRequest(response);
            }

            var success = await handler.HandleAsync(command);
            if (!success)
            {
                return Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Card not found."));
            }
            return Results.Ok(ApiResponse<object>.Ok(new { id }));
        })
        .WithName("MoveCard")
        .WithTags("Cards");
    }
}