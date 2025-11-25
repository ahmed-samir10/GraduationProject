namespace KonSanad.Core.Dtos
{
    public class DonationItemDto
    {
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
        public string? CustomName { get; set; }  // لو الصنف مش موجود في المخزن (مثل: ملابس شتوي)
        public string Unit { get; set; } = "وحدة";
    }
}