namespace KonSanad.APIS.DTOs
{
    public class CampaignListDTO
    {
        public List<CampaignCardDTO> Campaigns { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
