using Boardsync.Api.Common.Models;
using Boardsync.Api.Common.Auth;
using System.Security.Claims;

namespace Boardsync.Api.Features.Boards.GetBoards;


public static class GetBoardsEndpoint
{
    public static void MapGetBoardsEndpoint(this IEndpointRouteBuilder app)
    { 
        app.MapGet("/api/boards", async (
            ClaimsPrincipal principal,
            GetBoardsHandler handler) =>
            {
                var query = new GetBoardsQuery
                {
                    OwnerId = principal.GetRequiredUserId()
                };

                // 2. Execute Handler
                var boards = await handler.HandleAsync(query);

                // 3. Return Standard Response
                return Results.Ok(ApiResponse<object>.Ok(boards));
            })
            .RequireAuthorization()
            .WithName("GetBoards")
            .WithTags("Boards");
    }
}
