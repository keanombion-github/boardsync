using System.Security.Claims;
using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Models;

namespace Boardsync.Api.Features.Cards.AssignCard;

public static class AssignCardEndpoint
{
    public static void MapAssignCardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/cards/{id:guid}/assignee", async (
            Guid id, AssignCardBody request, ClaimsPrincipal principal,
            AssignCardHandler handler) =>
        {
            var result = await handler.HandleAsync(
                id, principal.GetRequiredUserId(), request.AssigneeId);
            return result switch
            {
                AssignCardResult.Assigned => Results.Ok(
                    ApiResponse<object>.Ok(new { id, request.AssigneeId })),
                AssignCardResult.InvalidAssignee => Results.BadRequest(
                    ApiResponse<object>.Fail("INVALID_ASSIGNEE", "Choose someone on this board.")),
                _ => Results.NotFound(
                    ApiResponse<object>.Fail("NOT_FOUND", "Card not found."))
            };
        }).RequireAuthorization().WithName("AssignCard").WithTags("Cards");
    }

    public sealed record AssignCardBody(Guid? AssigneeId);
}
