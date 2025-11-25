namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryStatisticsDTO
    {
        public int TotalBeneficiaries { get; set; }
        public int ActiveBeneficiaries { get; set; }
        public int TotalRequests { get; set; }
        public int FulfilledRequests { get; set; }
        public decimal TotalValueDistributed { get; set; }

        // توزيع حسب النوع
        public Dictionary<string, int> BeneficiariesByType { get; set; } = new();

        // توزيع حسب الحالة
        public Dictionary<string, int> BeneficiariesByCaseType { get; set; } = new();
    }
}
