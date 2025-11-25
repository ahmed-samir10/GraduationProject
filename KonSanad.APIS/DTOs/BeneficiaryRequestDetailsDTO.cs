namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryRequestDetailsDTO
    {
        public int Id { get; set; }

        // معلومات المستفيد
        public int BeneficiaryId { get; set; }
        public string BeneficiaryName { get; set; } = null!;
        public string BeneficiaryPhone { get; set; } = null!;
        public string BeneficiaryAddress { get; set; } = null!;
        public string BeneficiaryType { get; set; } = null!;
        public string? CaseType { get; set; }
        public int? FamilyMembersCount { get; set; }

        // تفاصيل الطلب
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal? EstimatedCost { get; set; }
        public string Priority { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime RequestDate { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public string? Notes { get; set; }

        // العناصر المطلوبة
        public List<RequiredItemDTO>? RequiredItems { get; set; }

        // سجل الموافقات/الرفض
        public List<RequestActionHistoryDTO>? ActionHistory { get; set; }
    }
}
