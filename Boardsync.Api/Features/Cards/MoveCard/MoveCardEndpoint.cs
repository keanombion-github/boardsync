using Boardsync.Api.Common.Models;
using Boardsync.Api.Common.Auth;
using FluentValidation;
using System.Security.Claims;

namespace Boardsync.Api.Features.Cards.MoveCard;

public static class MoveCardEndpoint
{
    public static void MapMoveCardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/cards/{id}/move", async (
            Guid id,
            MoveCardBody body,
            ClaimsPrincipal principal,
            IValidator<MoveCardCommand> validator,
            MoveCardHandler handler
        ) =>
        {
            var command = new MoveCardCommand
            {
                Id = id,
                UserId = principal.GetRequiredUserId(),
                ColumnId = body.ColumnId,
                BeforeCardId = body.BeforeCardId,
                AfterCardId = body.AfterCardId
            };

            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid card data.");
                return Results.BadRequest(response);
            }

            var result = await handler.HandleAsync(command);
            
            if (result == MoveCardResult.CardNotFound)
                return Results.NotFound(
                    ApiResponse<object>.Fail("NOT_FOUND", "Card not found.")
                );

            if (result == MoveCardResult.DestinationColumnNotFound)
                return Results.NotFound(
                    ApiResponse<object>.Fail("NOT_FOUND", "Destination column not found.")
                );

            if (result == MoveCardResult.CrossBoardMove)
                return Results.BadRequest(
                    ApiResponse<object>.Fail(
                        "INVALID_DESTINATION",
                        "Cards cannot be moved between unrelated boards."
                    )
                );

            if (result == MoveCardResult.InvalidNeighbors)
                return Results.BadRequest(
                    ApiResponse<object>.Fail(
                        "INVALID_NEIGHBORS",
                        "Card neighbors must be adjacent cards in the destination column."
                    )
                );

            return Results.Ok(ApiResponse<object>.Ok(new { id }));
        })
        .RequireAuthorization()
        .WithName("MoveCard")
        .WithTags("Cards");
    }
}
