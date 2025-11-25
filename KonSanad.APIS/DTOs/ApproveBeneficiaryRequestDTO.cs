using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class ApproveBeneficiaryRequestDTO
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = null!; // "Approve", "Reject", "UnderReview"

        [MaxLength(500)]
        public string? Notes { get; set; }

        // إذا تمت الموافقة، هل سيتم إنشاء حملة؟
        public bool CreateCampaign { get; set; } = false;

        // تفاصيل الحملة (إذا CreateCampaign = true)
        public CreateCampaignFromRequestDTO? CampaignDetails { get; set; }
    }
}
