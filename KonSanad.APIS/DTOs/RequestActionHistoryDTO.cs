namespace KonSanad.APIS.DTOs
{
    public class RequestActionHistoryDTO
    {
        public int Id { get; set; }
        public string Action { get; set; } = null!; // "Approved", "Rejected", "UnderReview"
        public string? ManagerName { get; set; }
        public DateTime ActionDate { get; set; }
        public string? Notes { get; set; }
    }
}
