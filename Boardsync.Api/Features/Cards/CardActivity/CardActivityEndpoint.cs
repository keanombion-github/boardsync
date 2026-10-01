using System.Security.Claims;
using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Models;

namespace Boardsync.Api.Features.Cards.CardActivity;

public static class CardActivityEndpoint
{
    private static readonly HashSet<string> AllowedReactions = ["👍", "❤️", "🎉", "👀"];

    public static void MapCardActivityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cards/{id:guid}/activity", async (
            Guid id, ClaimsPrincipal principal, CardActivityHandler handler) =>
        {
            var activity = await handler.GetAsync(id, principal.GetRequiredUserId());
            return activity is null
                ? Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Card not found."))
                : Results.Ok(ApiResponse<CardActivityDto>.Ok(activity));
        }).RequireAuthorization().WithName("GetCardActivity").WithTags("Cards");

        app.MapPost("/api/cards/{id:guid}/comments", async (
            Guid id, CommentBody request, ClaimsPrincipal principal,
            CardActivityHandler handler) =>
        {
            var body = request.Body?.Trim();
            if (string.IsNullOrWhiteSpace(body) || body.Length > 2000)
                return Results.BadRequest(ApiResponse<object>.Fail(
                    "VALIDATION_ERROR", "Comment must contain 1 to 2000 characters."));

            var commentId = await handler.AddCommentAsync(
                id, principal.GetRequiredUserId(), body);
            return commentId is null
                ? Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Card not found."))
                : Results.Json(ApiResponse<object>.Ok(new { id = commentId.Value }),
                    statusCode: StatusCodes.Status201Created);
        }).RequireAuthorization().WithName("AddCardComment").WithTags("Cards");

        app.MapPut("/api/cards/{id:guid}/reactions", async (
            Guid id, ReactionBody request, ClaimsPrincipal principal,
            CardActivityHandler handler) =>
        {
            if (!AllowedReactions.Contains(request.Emoji ?? ""))
                return Results.BadRequest(ApiResponse<object>.Fail(
                    "VALIDATION_ERROR", "Choose a supported reaction."));

            var found = await handler.ToggleReactionAsync(
                id, principal.GetRequiredUserId(), request.Emoji!);
            return found
                ? Results.Ok(ApiResponse<object>.Ok(new { id }))
                : Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Card not found."));
        }).RequireAuthorization().WithName("ToggleCardReaction").WithTags("Cards");

        app.MapPost("/api/cards/{id:guid}/attachments", async (
            Guid id, AttachmentBody request, ClaimsPrincipal principal,
            CardActivityHandler handler) =>
        {
            var label = request.Label?.Trim();
            var url = request.Url?.Trim();
            if (string.IsNullOrWhiteSpace(label) || label.Length > 120
                || string.IsNullOrWhiteSpace(url) || url.Length > 2048
                || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                || !string.IsNullOrEmpty(uri.UserInfo))
                return Results.BadRequest(ApiResponse<object>.Fail(
                    "VALIDATION_ERROR", "Provide a label and an HTTP or HTTPS link."));

            var attachmentId = await handler.AddAttachmentAsync(
                id, principal.GetRequiredUserId(), label, url);
            return attachmentId is null
                ? Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Card not found."))
                : Results.Json(ApiResponse<object>.Ok(new { id = attachmentId.Value }),
                    statusCode: StatusCodes.Status201Created);
        }).RequireAuthorization().WithName("AddCardAttachment").WithTags("Cards");

        app.MapDelete("/api/cards/{id:guid}/attachments/{attachmentId:guid}", async (
            Guid id, Guid attachmentId, ClaimsPrincipal principal,
            CardActivityHandler handler) =>
        {
            var deleted = await handler.DeleteAttachmentAsync(
                id, attachmentId, principal.GetRequiredUserId());
            return deleted
                ? Results.NoContent()
                : Results.NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Attachment not found."));
        }).RequireAuthorization().WithName("DeleteCardAttachment").WithTags("Cards");
    }

    public sealed record CommentBody(string? Body);
    public sealed record ReactionBody(string? Emoji);
    public sealed record AttachmentBody(string? Label, string? Url);
}
