using System.Threading.Channels;
using TestFirstProject.Constants;
using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Models;
using TestFirstProject.Services;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    public static class ConversationEndpoints
    {
        public static void MapConversationEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/conversations")
                .RequireAuthorization();

            group.MapPost("/", async (
                StartConversationRequest request,
                IMessageService messageService,
                Channel<NotificationEvent> notificationChannel,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();

                var conversation = await messageService.StartOrGetConversationAsync(userId, request.RecipientUserId);

                await notificationChannel.PublishNotificationAsync(
                    request.RecipientUserId,
                    NotificationType.ConversationStarted,
                    "New Conversation",
                    "A user started a conversation with you.",
                    conversation.Id.ToString());

                return Results.Ok(conversation);
            });

            group.MapGet("/", async (
                IMessageService messageService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                var conversations = await messageService.GetConversationsAsync(userId);
                return Results.Ok(conversations);
            });

            group.MapGet("/{id:guid}/messages", async (
                Guid id,
                IMessageService messageService,
                HttpContext httpContext,
                string? cursor,
                int? pageSize) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                var messages = await messageService.GetMessagesAsync(
                    userId, id, cursor, pageSize ?? 50);
                return Results.Ok(messages);
            });

            group.MapPost("/{id:guid}/messages", async (
                Guid id,
                SendMessageRequest request,
                IMessageService messageService,
                ISignalRNotifier signalRNotifier,
                Channel<NotificationEvent> notificationChannel,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();

                var message = await messageService.SendMessageAsync(userId, id, request.Content);

                var recipientId = await messageService.GetRecipientUserIdAsync(userId, id);

                await signalRNotifier.NotifyNewMessageAsync(recipientId, message);

                var preview = message.Content.Length > 100
                    ? message.Content[..100] + "..."
                    : message.Content;

                await notificationChannel.PublishNotificationAsync(
                    recipientId,
                    NotificationType.NewMessage,
                    "New Message",
                    $"{message.SenderName}: {preview}",
                    id.ToString());

                return Results.Created($"/conversations/{id}/messages/{message.Id}", message);
            })
            .RequireRateLimiting(PolicyNames.MessageRateLimit);

            group.MapPut("/{id:guid}/messages/{messageId:guid}/read", async (
                Guid id,
                Guid messageId,
                IMessageService messageService,
                ISignalRNotifier signalRNotifier,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                await messageService.MarkMessageAsReadAsync(userId, id, messageId);

                var senderId = await messageService.GetRecipientUserIdAsync(userId, id);
                await signalRNotifier.NotifyMessageReadAsync(senderId, id, messageId);

                return Results.NoContent();
            });

            group.MapDelete("/{id:guid}/messages/{messageId:guid}", async (
                Guid id,
                Guid messageId,
                IMessageService messageService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                await messageService.SoftDeleteMessageAsync(userId, id, messageId);
                return Results.NoContent();
            });
        }
    }
}
