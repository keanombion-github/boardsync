using Boardsync.Api.Common.Models;

namespace Boardsync.Api.Features.Cards.DeleteCard;

public static class DeleteCardEndpoint
{
    public static void MapDeleteCardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/cards/{id}", async (
            Guid id,
            DeleteCardHandler handler
        ) =>
        {
            var command = new DeleteCardCommand
            {
                Id = id
            };

            var deleted = await handler.HandleAsync(command);
            if (!deleted)
                return Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Card not found."));
            return Results.NoContent();
        })
        .WithName("DeleteCard")
        .WithTags("Cards");
    }
}