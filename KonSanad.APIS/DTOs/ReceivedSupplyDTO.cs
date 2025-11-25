namespace KonSanad.APIS.DTOs
{
    public class ReceivedSupplyDTO
    {
        public int Id { get; set; }
        public string SupplyName { get; set; } = null!;
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public DateTime ReceivedDate { get; set; }
        public string? DonorName { get; set; }
        public string Status { get; set; } = null!; // "InStock", "Distributed", "Reserved"
    }
}
