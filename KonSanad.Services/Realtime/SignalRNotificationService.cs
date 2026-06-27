using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace KonSanad.Services.Realtime
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IDatabase _redis;
        private const int MaxNotificationsPerChannel = 200;

        public SignalRNotificationService(
            IHubContext<NotificationHub> hubContext,
            IConnectionMultiplexer connectionMultiplexer)
        {
            _hubContext = hubContext;
            _redis = connectionMultiplexer.GetDatabase();
        }

        public Task NotifyManagersAsync(NotificationMessageDto notification)
            => NotifyRoleAsync("Manager", notification);

        public async Task NotifyRoleAsync(string role, NotificationMessageDto notification)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return;
            }

            var payload = Clone(notification);
            payload.RecipientRole = role;

            await SaveRoleNotificationAsync(role, payload);
            await _hubContext.Clients
                .Group(NotificationHub.GetRoleGroup(role))
                .SendAsync(NotificationHub.ReceiveNotificationMethod, payload);
        }

        public async Task NotifyUserAsync(string appUserId, NotificationMessageDto notification)
        {
            if (string.IsNullOrWhiteSpace(appUserId))
            {
                return;
            }

            var payload = Clone(notification);

            await SaveUserNotificationAsync(appUserId, payload);
            await _hubContext.Clients
                .Group(NotificationHub.GetUserGroup(appUserId))
                .SendAsync(NotificationHub.ReceiveNotificationMethod, payload);
        }

        public async Task NotifyUsersAsync(IEnumerable<string> appUserIds, NotificationMessageDto notification)
        {
            var userIds = appUserIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (userIds.Count == 0)
            {
                return;
            }

            foreach (var userId in userIds)
            {
                var payload = Clone(notification);
                await SaveUserNotificationAsync(userId, payload);
                await _hubContext.Clients
                    .Group(NotificationHub.GetUserGroup(userId))
                    .SendAsync(NotificationHub.ReceiveNotificationMethod, payload);
            }
        }

        public async Task<IReadOnlyList<NotificationMessageDto>> GetCurrentUserNotificationsAsync(
            string appUserId,
            IEnumerable<string> roles)
        {
            var notifications = new List<NotificationMessageDto>();

            if (!string.IsNullOrWhiteSpace(appUserId))
            {
                notifications.AddRange(await ReadNotificationsAsync(GetUserKey(appUserId)));
            }

            foreach (var role in roles.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                notifications.AddRange(await ReadNotificationsAsync(GetRoleKey(role)));
            }

            return notifications
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.Id)
               
                .ToList();
        }

        private async Task SaveRoleNotificationAsync(string role, NotificationMessageDto notification)
        {
            var key = GetRoleKey(role);
            await SaveNotificationAsync(key, notification);
        }

        private async Task SaveUserNotificationAsync(string appUserId, NotificationMessageDto notification)
        {
            var key = GetUserKey(appUserId);
            await SaveNotificationAsync(key, notification);
        }

        private async Task SaveNotificationAsync(string key, NotificationMessageDto notification)
        {
            var json = JsonSerializer.Serialize(notification);
            await _redis.ListLeftPushAsync(key, json);
            await _redis.ListTrimAsync(key, 0, MaxNotificationsPerChannel - 1);
        }

        private async Task<List<NotificationMessageDto>> ReadNotificationsAsync(string key)
        {
            var values = await _redis.ListRangeAsync(key, 0, MaxNotificationsPerChannel - 1);
            return values
                .Where(x => x.HasValue)
                .Select(x => JsonSerializer.Deserialize<NotificationMessageDto>(x!))
                .Where(x => x != null)
                .ToList()!;
        }

        private static NotificationMessageDto Clone(NotificationMessageDto notification)
        {
            return new NotificationMessageDto
            {
                Id = Guid.NewGuid().ToString(),
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                RecipientRole = notification.RecipientRole,
                CreatedAtUtc = notification.CreatedAtUtc == default ? DateTime.UtcNow : notification.CreatedAtUtc,
                Data = notification.Data == null
                    ? null
                    : new Dictionary<string, string>(notification.Data)
            };
        }
        public async Task MarkAsReadAsync(string appUserId, IEnumerable<string> roles, string notificationId)
        {
            // اتحدث في user key
            await UpdateIsReadAsync(GetUserKey(appUserId), notificationId);

            // اتحدث في كل role keys
            foreach (var role in roles.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                await UpdateIsReadAsync(GetRoleKey(role), notificationId);
            }
        }
        public async Task MarkAllAsReadAsync(string appUserId, IEnumerable<string> roles)
        {
            // Notifications الخاصة باليوزر
            await MarkAllByKeyAsReadAsync(GetUserKey(appUserId));

            // Notifications الخاصة بالـ Roles
            foreach (var role in roles.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                await MarkAllByKeyAsReadAsync(GetRoleKey(role));
            }
        }

        private async Task MarkAllByKeyAsReadAsync(string key)
        {
            var values = await _redis.ListRangeAsync(key, 0, MaxNotificationsPerChannel - 1);

            for (int i = 0; i < values.Length; i++)
            {
                if (!values[i].HasValue)
                    continue;

                var notification =
                    JsonSerializer.Deserialize<NotificationMessageDto>(values[i]!);

                if (notification == null || notification.isRead)
                    continue;

                notification.isRead = true;

                await _redis.ListSetByIndexAsync(
                    key,
                    i,
                    JsonSerializer.Serialize(notification));
            }
        }
        private async Task UpdateIsReadAsync(string key, string notificationId)
        {
            var values = await _redis.ListRangeAsync(key, 0, MaxNotificationsPerChannel - 1);

            for (int i = 0; i < values.Length; i++)
            {
                if (!values[i].HasValue) continue;

                var notification = JsonSerializer.Deserialize<NotificationMessageDto>(values[i]!);
                if (notification?.Id != notificationId) continue;

                notification.isRead = true;
                var updated = JsonSerializer.Serialize(notification);
                await _redis.ListSetByIndexAsync(key, i, updated);
                break;
            }
        }

        private static string GetUserKey(string appUserId)
            => $"notifications:user:{appUserId}";

        private static string GetRoleKey(string role)
            => $"notifications:role:{role.Trim().ToLowerInvariant()}";
    }
}
