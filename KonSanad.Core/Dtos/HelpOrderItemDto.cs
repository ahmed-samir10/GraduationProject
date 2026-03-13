using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.Dtos
{
    public class HelpOrderItemDto
    {
        public int Id { get; set; }
        public int? HelpOrderId { get; set; }
        public RequestedStatus Status { get; set; }
        public int? SupplyId { get; set; }
        public string Title { get; set; }
        public int? Quantity { get; set; }
        public string? Reason { get; set; }
        public string? ImageUrl { get; set; } // optional image url for this help-order item
    }
}