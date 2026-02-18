using KonSanad.Repository.Entities;

namespace KonSanad.Core.Entity
{
    public class CartItem : BaseEntity<int>
    {
        public int CartId { get; set; }
        public Cart Cart { get; set; }

        public int SupplyId { get; set; }
        public string imageUrl { get; set; }
        public Supply supply { get; set; }

        public int Quantity { get; set; }
    }
}