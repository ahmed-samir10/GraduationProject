using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class IssueReceiptDTO
    {
        [Required]
        public int DonationOrderId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
