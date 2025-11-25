namespace KonSanad.APIS.DTOs
{
    public class DonationDetailsForManagerDTO
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public string DonationType { get; set; } = null!;
        public string Status { get; set; } = null!;

        // معلومات المتبرع
        public int DonorId { get; set; }
        public string DonorName { get; set; } = null!;
        public string DonorEmail { get; set; } = null!;
        public string DonorPhone { get; set; } = null!;
        public string DonorAddress { get; set; } = null!;
        public string DonorType { get; set; } = null!;

        // معلومات الحملة
        public int? CampaignId { get; set; }
        public string? CampaignTitle { get; set; }

        // تفاصيل التبرع النقدي
        public decimal? CashAmount { get; set; }

        // تفاصيل التبرع العيني
        public List<OrderItemDetailDTO>? Items { get; set; }

        // الإيصال
        public int? ReceiptId { get; set; }
        public DateTime? ReceiptIssuedDate { get; set; }

        public string? Notes { get; set; }
    }
}
