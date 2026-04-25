using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.Dtos
{
    public class NotificationMessageDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? RecipientRole { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string>? Data { get; set; }
        public bool isRead { get; set; } = false;
    }
}
