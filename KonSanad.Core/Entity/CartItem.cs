using KonSanad.Repository.Entities;

namespace KonSanad.Core.Entity
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public Cart Cart { get; set; }

        public int ProductId { get; set; }
        public Supply Supply  { get; set; }

        public int Quantity { get; set; } = 1;

        public string? ImageUrl { get; set; }
    }
}