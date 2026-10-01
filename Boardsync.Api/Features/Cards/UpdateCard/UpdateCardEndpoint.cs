using Boardsync.Api.Common.Models;
using Boardsync.Api.Common.Auth;
using FluentValidation;
using System.Security.Claims;

namespace Boardsync.Api.Features.Cards.UpdateCard;

public static class UpdateCardEndpoint {
    public static void MapUpdateCardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/cards/{id}", async (
            Guid id,
            UpdateCardBody body,
            ClaimsPrincipal principal,
            IValidator<UpdateCardCommand> validator,
            UpdateCardHandler handler
        ) => {
            
            var command = new UpdateCardCommand(
                id,
                principal.GetRequiredUserId(),
                body.Title,
                body.Description);

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
        .RequireAuthorization()
        .WithName("UpdateCard")
        .WithTags("Cards");
    }
}
