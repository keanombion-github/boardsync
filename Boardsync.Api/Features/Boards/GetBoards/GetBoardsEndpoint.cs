using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Boards.GetBoards;


public static class GetBoardsEndpoint
{
    public static void MapGetBoardsEndpoint(this IEndpointRouteBuilder app)
    { 
        app.MapGet("/api/boards", async (
            [AsParameters] GetBoardsQuery query,
            GetBoardsHandler handler) =>
            {
                // 2. Execute Handler
                var boards = await handler.HandleAsync(query);

                // 3. Return Standard Response
                return Results.Ok(ApiResponse<object>.Ok(boards));
            })
            .WithName("GetBoards")
            .WithTags("Boards");
    }
}