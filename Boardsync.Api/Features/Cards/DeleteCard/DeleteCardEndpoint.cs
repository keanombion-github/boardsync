using Boardsync.Api.Common.Models;
using Boardsync.Api.Common.Auth;
using System.Security.Claims;

namespace Boardsync.Api.Features.Cards.DeleteCard;

public static class DeleteCardEndpoint
{
    public static void MapDeleteCardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/cards/{id}", async (
            Guid id,
            ClaimsPrincipal principal,
            DeleteCardHandler handler
        ) =>
        {
            var command = new DeleteCardCommand
            {
                Id = id,
                UserId = principal.GetRequiredUserId()
            };

            var deleted = await handler.HandleAsync(command);
            if (!deleted)
                return Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Card not found."));
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteCard")
        .WithTags("Cards");
    }
}
