namespace KonSanad.APIS.DTOs
{
    public class HomeStatisticsDTO
    {
        public int TotalVolunteers { get; set; }
        public decimal TotalDonations { get; set; }
        public int TotalBeneficiaries { get; set; }
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
    }
}
