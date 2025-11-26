using KonSanad.Core.Dtos;
namespace KonSanad.Core.Dtos
{
    public class CampaignDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Status { get; set; } // نشطة - عاجلة - مكتملة
        public int ProgressPercentage { get; set; }

        public BeneficiaryDto Beneficiary { get; set; } = new BeneficiaryDto();
        public List<SupplyDto> Supplies { get; set; } = new List<SupplyDto>();
    }
}