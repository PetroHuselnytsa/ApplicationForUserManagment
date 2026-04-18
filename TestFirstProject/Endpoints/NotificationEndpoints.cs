using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    public static class NotificationEndpoints
    {
        public static void MapNotificationEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/notifications")
                .RequireAuthorization();

            group.MapGet("/", async (
                INotificationService notificationService,
                HttpContext httpContext,
                int? page,
                int? pageSize) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                var notifications = await notificationService.GetNotificationsAsync(
                    userId, page ?? 1, pageSize ?? 20);
                return Results.Ok(notifications);
            });

            group.MapPut("/{id:guid}/read", async (
                Guid id,
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                await notificationService.MarkAsReadAsync(userId, id);
                return Results.NoContent();
            });

            group.MapPut("/read-all", async (
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                await notificationService.MarkAllAsReadAsync(userId);
                return Results.NoContent();
            });

            group.MapDelete("/{id:guid}", async (
                Guid id,
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                await notificationService.DeleteNotificationAsync(userId, id);
                return Results.NoContent();
            });

            group.MapGet("/preferences", async (
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                var preferences = await notificationService.GetPreferencesAsync(userId);
                return Results.Ok(preferences);
            });

            group.MapPut("/preferences", async (
                UpdateNotificationPreferenceRequest request,
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetRequiredUserGuid();
                var preference = await notificationService.UpdatePreferenceAsync(
                    userId, request.NotificationType, request.IsEnabled);
                return Results.Ok(preference);
            });
        }
    }
}
