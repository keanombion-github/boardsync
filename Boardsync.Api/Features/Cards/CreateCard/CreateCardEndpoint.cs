using Boardsync.Api.Common.Models;
using Boardsync.Api.Common.Auth;
using FluentValidation;
using System.Security.Claims;

namespace Boardsync.Api.Features.Cards.CreateCard;

public static class CreateCardEndpoint
{
    public static void MapCreateCardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cards", async (
            CreateCardBody body,
            ClaimsPrincipal principal,
            IValidator<CreateCardCommand> validator,
            CreateCardHandler handler
        ) =>
        {
            var command = new CreateCardCommand
            {
                Title = body.Title,
                Description = body.Description,
                ColumnId = body.ColumnId,
                UserId = principal.GetRequiredUserId()
            };

            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid card data.");
                return Results.BadRequest(response);
            }

            var cardId = await handler.HandleAsync(command);

            if (!cardId.HasValue)
                return Results.NotFound(
                    ApiResponse<object>.Fail("NOT_FOUND", "Column not found."));

            return Results.Json(
                ApiResponse<object>.Ok(new { id = cardId.Value }),
                statusCode: StatusCodes.Status201Created);
        })
        .RequireAuthorization()
        .WithName("CreateCard")
        .WithTags("Cards");
    }
}
