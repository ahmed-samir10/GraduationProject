namespace KonSanad.APIS.DTOs
{
    public class OrderItemReadDTO
    {
        public int Id { get; set; }
        public string CustomSupplyName { get; set; } = null!;
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public string? SupplyName { get; set; }
    }
}
