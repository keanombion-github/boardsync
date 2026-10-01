using Boardsync.Api.Common.Models;
using Boardsync.Api.Common.Auth;
using FluentValidation;
using System.Security.Claims;

namespace Boardsync.Api.Features.Columns.CreateColumn;

public static class CreateColumnEndpoint
{
    public static void MapCreateColumnEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/boards/{boardId}/columns", async (
            Guid boardId,
            CreateColumnBody body,
            ClaimsPrincipal principal,
            IValidator<CreateColumnCommand> validator,
            CreateColumnHandler handler
        ) =>
        {
            var command = new CreateColumnCommand
            {
                Name = body.Name,
                BoardId = boardId,
                UserId = principal.GetRequiredUserId()
            };
            var validationResult = await validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var response = ApiResponse<object>.Fail("VALIDATION_ERROR", "Invalid column data.");
                return Results.BadRequest(response);
            }

            var columnId = await handler.HandleAsync(command);

            if (!columnId.HasValue)
                return Results.NotFound(
                    ApiResponse<object>.Fail("NOT_FOUND", "Board not found."));

            return Results.Json(
                ApiResponse<object>.Ok(new { Id = columnId.Value }),
                statusCode: StatusCodes.Status201Created);
        })
        .RequireAuthorization()
        .WithName("CreateColumn")
        .WithTags("Columns");
    }
}
