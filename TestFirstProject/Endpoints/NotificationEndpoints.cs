using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps notification endpoints.
    /// All endpoints require authentication and enforce that users can only access their own notifications.
    /// </summary>
    public static class NotificationEndpoints
    {
        public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/notifications")
                           .WithTags("Notifications")
                           .RequireAuthorization();

            // GET /notifications — paginated list for current user
            group.MapGet("/", async (
                INotificationService notificationService,
                HttpContext httpContext,
                int page = 1,
                int pageSize = 20) =>
            {
                var userId = httpContext.GetUserId();
                var result = await notificationService.GetNotificationsAsync(userId, page, pageSize);
                return Results.Ok(result);
            })
            .WithName("GetNotifications");

            // PUT /notifications/{id}/read — mark single as read
            group.MapPut("/{id:guid}/read", async (
                Guid id,
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                await notificationService.MarkAsReadAsync(userId, id);
                return Results.NoContent();
            })
            .WithName("MarkNotificationAsRead");

            // PUT /notifications/read-all — mark all as read
            group.MapPut("/read-all", async (
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                await notificationService.MarkAllAsReadAsync(userId);
                return Results.NoContent();
            })
            .WithName("MarkAllNotificationsAsRead");

            // DELETE /notifications/{id} — delete notification
            group.MapDelete("/{id:guid}", async (
                Guid id,
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                await notificationService.DeleteNotificationAsync(userId, id);
                return Results.NoContent();
            })
            .WithName("DeleteNotification");

            // GET /notifications/preferences — get notification preferences
            group.MapGet("/preferences", async (
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await notificationService.GetPreferencesAsync(userId);
                return Results.Ok(result);
            })
            .WithName("GetNotificationPreferences");

            // PUT /notifications/preferences — update notification preferences
            group.MapPut("/preferences", async (
                UpdateNotificationPreferenceRequest request,
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                await notificationService.UpdatePreferencesAsync(userId, request);
                return Results.NoContent();
            })
            .WithName("UpdateNotificationPreferences");
        }

    }
}
