namespace KonSanad.Core.Dtos
{
    public class CartItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
      public string ImagUrl { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public string Catgorey { get; set; }
    }

}