namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryNotificationDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;

        // "RequestApproved", "CampaignCreated", "DonationReceived"
        public string Type { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
