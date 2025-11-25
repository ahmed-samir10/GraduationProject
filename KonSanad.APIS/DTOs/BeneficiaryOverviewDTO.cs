namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryOverviewDTO
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int FulfilledRequests { get; set; }
        public decimal TotalReceivedValue { get; set; }
        public int ActiveCampaigns { get; set; }
    }
}
