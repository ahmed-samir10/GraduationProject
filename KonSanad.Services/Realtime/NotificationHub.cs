using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace KonSanad.Services.Realtime
{
    [EnableCors("AllowAll")]
    public class NotificationHub : Hub
    {
        public const string ReceiveNotificationMethod = "ReceiveNotification";

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(userId));
            }

            var roles = Context.User?
                .FindAll(ClaimTypes.Role)
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                ?? Enumerable.Empty<string>();

            foreach (var role in roles)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetRoleGroup(role));
            }

            await base.OnConnectedAsync();
        }

        public static string GetUserGroup(string appUserId)
            => $"user:{appUserId}";

        public static string GetRoleGroup(string role)
            => $"role:{role.Trim().ToLowerInvariant()}";
    }
}
