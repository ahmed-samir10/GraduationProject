namespace KonSanad.APIS.DTOs
{
    public class ManagerDashboardDTO
    {
        // الإحصائيات العامة
        public DashboardStatisticsDTO Statistics { get; set; } = null!;

        // الحملات المميزة
        public List<CampaignCardDTO> FeaturedCampaigns { get; set; } = new();

        // الحملات العاجلة
        public List<CampaignCardDTO> UrgentCampaigns { get; set; } = new();

        // المتطلبات الملحة
        public List<RequirementCardDTO> UrgentRequirements { get; set; } = new();

        // التبرعات الأخيرة (تحتاج موافقة/معالجة)
        public List<PendingDonationDTO> PendingDonations { get; set; } = new();

        // طلبات المستفيدين (تحتاج موافقة)
        public List<PendingBeneficiaryRequestDTO> PendingBeneficiaryRequests { get; set; } = new();

        // طلبات التطوع (تحتاج موافقة)
        public List<PendingVolunteerRequestDTO> PendingVolunteerRequests { get; set; } = new();
    }
}
