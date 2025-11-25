namespace KonSanad.APIS.DTOs
{
    public class CategoryReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int CampaignsCount { get; set; }
    }
}
