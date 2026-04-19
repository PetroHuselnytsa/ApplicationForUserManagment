using Microsoft.AspNetCore.RateLimiting;
using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps conversation and message endpoints.
    /// All endpoints require authentication and enforce that users can only access their own conversations.
    /// </summary>
    public static class ConversationEndpoints
    {
        public static void MapConversationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/conversations")
                           .WithTags("Conversations")
                           .RequireAuthorization();

            // POST /conversations — start or get existing conversation with a user
            group.MapPost("/", async (StartConversationRequest request, IMessageService messageService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await messageService.StartOrGetConversationAsync(userId, request.ParticipantId);
                return Results.Ok(result);
            })
            .WithName("StartOrGetConversation");

            // GET /conversations — list all conversations for current user
            group.MapGet("/", async (IMessageService messageService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await messageService.GetConversationsAsync(userId);
                return Results.Ok(result);
            })
            .WithName("GetConversations");

            // GET /conversations/{id}/messages — paginated message history (cursor-based)
            group.MapGet("/{id:guid}/messages", async (
                Guid id,
                IMessageService messageService,
                HttpContext httpContext,
                string? cursor,
                int pageSize = 50) =>
            {
                var userId = httpContext.GetUserId();
                var result = await messageService.GetMessagesAsync(userId, id, cursor, pageSize);
                return Results.Ok(result);
            })
            .WithName("GetMessages");

            // POST /conversations/{id}/messages — send a message
            group.MapPost("/{id:guid}/messages", async (
                Guid id,
                SendMessageRequest request,
                IMessageService messageService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await messageService.SendMessageAsync(userId, id, request.Content);
                return Results.Created($"/api/conversations/{id}/messages/{result.Id}", result);
            })
            .WithName("SendMessage")
            .RequireRateLimiting("messaging");

            // PUT /conversations/{id}/messages/{messageId}/read — mark as read
            group.MapPut("/{id:guid}/messages/{messageId:guid}/read", async (
                Guid id,
                Guid messageId,
                IMessageService messageService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                await messageService.MarkMessageAsReadAsync(userId, id, messageId);
                return Results.NoContent();
            })
            .WithName("MarkMessageAsRead");

            // DELETE /conversations/{id}/messages/{messageId} — soft delete
            group.MapDelete("/{id:guid}/messages/{messageId:guid}", async (
                Guid id,
                Guid messageId,
                IMessageService messageService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                await messageService.DeleteMessageAsync(userId, id, messageId);
                return Results.NoContent();
            })
            .WithName("DeleteMessage");
        }

    }
}
