using KonSanad.Core.Dtos;

namespace KonSanad.Core.IRepositery
{
    public interface INotificationService
    {
        Task NotifyManagersAsync(NotificationMessageDto notification);
        Task NotifyRoleAsync(string role, NotificationMessageDto notification);
        Task NotifyUserAsync(string appUserId, NotificationMessageDto notification);
        Task NotifyUsersAsync(IEnumerable<string> appUserIds, NotificationMessageDto notification);
        Task<IReadOnlyList<NotificationMessageDto>> GetCurrentUserNotificationsAsync(
            string appUserId,
            IEnumerable<string> roles);
        Task MarkAsReadAsync(string appUserId, IEnumerable<string> roles, string notificationId);
        Task MarkAllAsReadAsync(string appUserId, IEnumerable<string> roles);
    }
}
