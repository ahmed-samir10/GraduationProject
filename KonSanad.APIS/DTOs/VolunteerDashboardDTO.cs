namespace KonSanad.APIS.DTOs
{
    public class VolunteerDashboardDTO
    {
        // نظرة عامة
        public VolunteerOverviewDTO Overview { get; set; } = null!;

        // الحملات النشطة المنضم إليها
        public List<VolunteerCampaignDTO> ActiveCampaigns { get; set; } = new();

        // المهام الحالية
        public List<VolunteerTaskDTO> CurrentTasks { get; set; } = new();

        // المنتجات المستلمة مؤخراً
        public List<ReceivedItemDTO> RecentReceivedItems { get; set; } = new();

        // الإشعارات
        public List<VolunteerNotificationDTO> RecentNotifications { get; set; } = new();

    }
}
