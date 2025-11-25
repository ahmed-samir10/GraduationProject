namespace KonSanad.APIS.DTOs
{
    public class ManagerReadDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;

        // إحصائيات المدير
        public int TotalCampaignsManaged { get; set; }
        public int ActiveCampaigns { get; set; }
        public int TotalDonationsProcessed { get; set; }
        public int TotalVolunteersManaged { get; set; }
        public decimal TotalCashManaged { get; set; }
    }
}
