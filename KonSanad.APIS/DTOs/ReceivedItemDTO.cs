namespace KonSanad.APIS.DTOs
{
    public class ReceivedItemDTO
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public DateTime ReceivedAt { get; set; }

        // معلومات الحملة
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;

        // معلومات التسليم
        public string? ReceivedFrom { get; set; } // اسم المسؤول
        public string Status { get; set; } = null!; // "Received", "Distributed", "Pending"
        public string? Condition { get; set; } // "New", "Used - Good"
        public string? Notes { get; set; }
    }
}
