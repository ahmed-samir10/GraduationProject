namespace KonSanad.APIS.DTOs
{
    public class PendingDonationDTO
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public string DonationType { get; set; } = null!; // "Cash", "InKind"
        public string Status { get; set; } = null!; // "Pending", "Approved", "Rejected", "Processed"

        // معلومات المتبرع
        public int DonorId { get; set; }
        public string DonorName { get; set; } = null!;
        public string DonorPhone { get; set; } = null!;
        public string DonorType { get; set; } = null!;

        // معلومات الحملة
        public int? CampaignId { get; set; }
        public string? CampaignTitle { get; set; }

        // تفاصيل التبرع
        public decimal? CashAmount { get; set; }
        public int? ItemsCount { get; set; }
        public string? Notes { get; set; }
    }
}
