using KonSanad.Core.Dtos;
using KonSanad.Core.Entity.Enums;
using KonSanad.Core.IRepositery;
using KonSanad.Services.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KonSanad.APIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("types")]
        public IActionResult GetNotificationTypes()
        {
            var types = Enum.GetValues<NotificationType>()
                .Select(x => new
                {
                    Id = (int)x,
                    Name = x.ToString()
                });

            return Ok(new
            {
                success = true,
                data = types
            });
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User id not found in token"
                });
            }

            var roles = User.FindAll(ClaimTypes.Role)
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var notifications = await _notificationService.GetCurrentUserNotificationsAsync(
                userId,
                roles,
                pageNumber,
                pageSize);

            return Ok(new
            {
                success = true,
                data = notifications,
                pageNumber,
                pageSize,
                roles
            });
        }

        [Authorize]
        [HttpGet("connection-info")]
        public IActionResult GetConnectionInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();

            return Ok(new
            {
                success = true,
                data = new
                {
                    hubUrl = "/hubs/notifications",
                    receiveMethod = NotificationHub.ReceiveNotificationMethod,
                    userId,
                    roles
                }
            });
        }

        [Authorize]
        [HttpPost("test/me")]
        public async Task<IActionResult> SendTestToCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User id not found in token"
                });
            }

            await _notificationService.NotifyUserAsync(userId, new NotificationMessageDto
            {
                Type = NotificationType.Unknown,
                Title = "Test Notification",
                Message = "This is a realtime test notification.",
                EntityType = "Test"
            });

            return Ok(new
            {
                success = true,
                message = "Test notification sent to current user"
            });
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("test/managers")]
        public async Task<IActionResult> SendTestToManagers()
        {
            await _notificationService.NotifyManagersAsync(new NotificationMessageDto
            {
                Type = NotificationType.Unknown,
                Title = "Manager Test Notification",
                Message = "This is a realtime test notification for managers.",
                EntityType = "Test"
            });

            return Ok(new
            {
                success = true,
                message = "Test notification sent to managers"
            });
        }
        [Authorize]
        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { success = false, message = "User id not found in token" });

            if (string.IsNullOrWhiteSpace(request?.NotificationId))
                return BadRequest(new { success = false, message = "NotificationId is required" });

            var roles = User.FindAll(ClaimTypes.Role)
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            await _notificationService.MarkAsReadAsync(userId, roles, request.NotificationId);

            return Ok(new { success = true });
        }
        [Authorize]
        [HttpPost("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new
                {
                    success = false,
                    message = "User id not found in token"
                });

            var roles = User.FindAll(ClaimTypes.Role)
                .Select(r => r.Value)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            await _notificationService.MarkAllAsReadAsync(userId, roles);

            return Ok(new
            {
                success = true,
                message = "All notifications marked as read"
            });
        }
    }
}
