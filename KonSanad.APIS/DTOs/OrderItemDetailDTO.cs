namespace KonSanad.APIS.DTOs
{
    public class OrderItemDetailDTO
    {
        public int Id { get; set; }
        public string CustomSupplyName { get; set; } = null!;
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public int SupplyId { get; set; }
        public string? SupplyName { get; set; }
        public string? Condition { get; set; }
        public string? Description { get; set; }
    }
}
