using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class UpdateTaskStatusDTO
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        [MaxLength(50)]
        public string NewStatus { get; set; } = null!; // "InProgress", "Completed"

        [MaxLength(500)]
        public string? CompletionNotes { get; set; }

        public int? HoursSpent { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
