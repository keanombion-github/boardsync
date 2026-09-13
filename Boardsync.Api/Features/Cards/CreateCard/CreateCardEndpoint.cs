using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Cards.CreateCard;

public static class CreateCardEndpoint {
    public static void MapCreateCardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cards", async (
            CreateCardBody body,
            IValidator<CreateCardCommand> validator,
            CreateCardHandler handler
        ) => {
            
            var command = new CreateCardCommand {
                Title = body.Title,
                Description = body.Description,
                ColumnId = body.ColumnId
            };

            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid card data.");
                return Results.BadRequest(response);
            }

            var cardId = await handler.HandleAsync(command);

            return Results.Created($"/api/cards/{cardId}", ApiResponse<object>.Ok(new { id = cardId}));
        })
        .WithName("CreateCard")
        .WithTags("Cards");
    }
}