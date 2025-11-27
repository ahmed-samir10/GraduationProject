namespace KonSanad.Core.Dtos
{
    public class HelpOrderItemDto
    {
        public int Id { get; set; }
        public int? HelpOrderId { get; set; }
        public int? SupplyId { get; set; }
        public int? Quantity { get; set; }
        public string? ImageUrl { get; set; } // optional image url for this help-order item
    }
}