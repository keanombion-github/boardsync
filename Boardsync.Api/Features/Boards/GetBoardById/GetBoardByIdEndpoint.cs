using Boardsync.Api.Common.Models;
using Boardsync.Api.Common.Auth;
using System.Security.Claims;

namespace Boardsync.Api.Features.Boards.GetBoardById;

public static class GetBoardByIdEndpoint
{
    public static void MapGetBoardByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/boards/{id}", async (
            Guid id,
            ClaimsPrincipal principal,
            GetBoardByIdHandler handler) =>
        {
            var query = new GetBoardByIdQuery
            {
                BoardId = id,
                UserId = principal.GetRequiredUserId()
            };
            var board = await handler.HandleAsync(query);

            if (board is null)
                return Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Board not found."));

            return Results.Ok(ApiResponse<object>.Ok(board));
        })
        .RequireAuthorization()
        .WithName("GetBoardById")
        .WithTags("Boards");
    }
}
