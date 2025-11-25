namespace KonSanad.APIS.DTOs
{
    public class ManagerActivityDTO
    {
        public int Id { get; set; }
        public string ActivityType { get; set; } = null!; // "CampaignCreated", "DonationProcessed", "ReceiptIssued"
        public string Description { get; set; } = null!;
        public DateTime ActivityDate { get; set; }
        public string? RelatedEntityType { get; set; } // "Campaign", "Donation", "Receipt"
        public int? RelatedEntityId { get; set; }
    }
}
