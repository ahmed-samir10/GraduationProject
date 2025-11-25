namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryReadDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string BeneficiaryType { get; set; } = null!;
        public string? CaseType { get; set; }
        public int? FamilyMembersCount { get; set; }

        // إحصائيات المستفيد
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int FulfilledRequests { get; set; }
        public decimal TotalReceivedValue { get; set; }
    }
}
