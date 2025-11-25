using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class ApproveVolunteerRequestDTO
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = null!; // "Approve", "Reject"

        [MaxLength(100)]
        public string? AssignedRole { get; set; } // إذا تمت الموافقة

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
