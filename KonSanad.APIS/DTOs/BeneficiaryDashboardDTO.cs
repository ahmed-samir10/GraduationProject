namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryDashboardDTO
    {
        // نظرة عامة
        public BeneficiaryOverviewDTO Overview { get; set; } = null!;

        // الطلبات النشطة
        public List<RequirementSummaryDTO> ActiveRequests { get; set; } = new();

        // الحملات المرتبطة
        public List<CampaignCardDTO> LinkedCampaigns { get; set; } = new();

        // آخر التحديثات
        public List<BeneficiaryNotificationDTO> RecentUpdates { get; set; } = new();
    }
}
