namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryProfileDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string BeneficiaryType { get; set; } = null!;
        public string? CaseType { get; set; }
        public int? FamilyMembersCount { get; set; }
        public string? CaseDescription { get; set; }

        // إحصائيات الملف الشخصي
        public int TotalRequests { get; set; }
        public int ActiveRequests { get; set; }
        public int CompletedRequests { get; set; }
        public decimal TotalReceivedCash { get; set; }
        public int TotalReceivedItems { get; set; }

        // الطلبات الأخيرة
        public List<RequirementSummaryDTO>? RecentRequests { get; set; }

        // الحملات المرتبطة
        public List<CampaignCardDTO>? RelatedCampaigns { get; set; }
    }
}
