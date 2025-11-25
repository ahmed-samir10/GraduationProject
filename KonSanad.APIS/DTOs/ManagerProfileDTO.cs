namespace KonSanad.APIS.DTOs
{
    public class ManagerProfileDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;

        // إحصائيات الملف الشخصي
        public int TotalCampaignsManaged { get; set; }
        public int ActiveCampaigns { get; set; }
        public int CompletedCampaigns { get; set; }
        public int TotalDonationsProcessed { get; set; }
        public int PendingDonations { get; set; }
        public int TotalBeneficiariesHelped { get; set; }
        public int TotalVolunteersManaged { get; set; }
        public decimal TotalCashManaged { get; set; }
        public int TotalReceiptsIssued { get; set; }

        // آخر النشاطات
        public List<ManagerActivityDTO>? RecentActivities { get; set; }

        // الحملات المنضم إليها
        public List<CampaignCardDTO>? ManagedCampaigns { get; set; }

        // المنتجات المستلمة
        public List<ReceivedSupplyDTO>? ReceivedSupplies { get; set; }

        // الطلبات المعلقة
        public int PendingBeneficiaryRequests { get; set; }
        public int PendingVolunteerRequests { get; set; }
    }
}
