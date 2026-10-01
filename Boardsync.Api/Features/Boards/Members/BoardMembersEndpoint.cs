using System.Security.Claims;
using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Models;

namespace Boardsync.Api.Features.Boards.Members;

public static class BoardMembersEndpoint
{
    public static void MapBoardMembersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/boards/{boardId:guid}/members", async (
            Guid boardId, ClaimsPrincipal principal, BoardMembersHandler handler) =>
        {
            var members = await handler.ListAsync(boardId, principal.GetRequiredUserId());
            return members is null
                ? Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Board not found."))
                : Results.Ok(ApiResponse<BoardMemberDto[]>.Ok(members));
        }).RequireAuthorization().WithName("GetBoardMembers").WithTags("Boards");

        app.MapPost("/api/boards/{boardId:guid}/members", async (
            Guid boardId, AddMemberBody request, ClaimsPrincipal principal,
            BoardMembersHandler handler) =>
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || email.Length > 255
                || !email.Contains('@'))
                return Results.BadRequest(ApiResponse<object>.Fail(
                    "VALIDATION_ERROR", "Enter a valid member email."));

            var result = await handler.AddAsync(
                boardId, principal.GetRequiredUserId(), email);
            return result switch
            {
                AddMemberResult.Added => Results.Json(
                    ApiResponse<object>.Ok(new { email }),
                    statusCode: StatusCodes.Status201Created),
                AddMemberResult.AlreadyMember => Results.Conflict(
                    ApiResponse<object>.Fail("ALREADY_MEMBER", "This user is already on the board.")),
                AddMemberResult.UserNotFound => Results.NotFound(
                    ApiResponse<object>.Fail("USER_NOT_FOUND", "No registered user has that email.")),
                _ => Results.NotFound(
                    ApiResponse<object>.Fail("NOT_FOUND", "Board not found."))
            };
        }).RequireAuthorization().WithName("AddBoardMember").WithTags("Boards");

        app.MapDelete("/api/boards/{boardId:guid}/members/{memberId:guid}", async (
            Guid boardId, Guid memberId, ClaimsPrincipal principal,
            BoardMembersHandler handler) =>
        {
            var removed = await handler.RemoveAsync(
                boardId, principal.GetRequiredUserId(), memberId);
            return removed
                ? Results.NoContent()
                : Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Board member not found."));
        }).RequireAuthorization().WithName("RemoveBoardMember").WithTags("Boards");
    }

    public sealed record AddMemberBody(string? Email);
}
