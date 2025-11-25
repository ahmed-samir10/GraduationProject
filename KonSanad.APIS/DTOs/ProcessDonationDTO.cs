using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class ProcessDonationDTO
    {
        [Required]
        public int DonationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = null!; // "Approve", "Reject"

        [MaxLength(500)]
        public string? Notes { get; set; }

        // إذا تمت الموافقة، هل سيتم إصدار إيصال؟
        public bool IssueReceipt { get; set; } = true;
    }
}
