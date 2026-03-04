using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class CartItemDto
    {
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // beneficiary | donationItem | donationCash | custom
      public string Reason { get; set; } = string.Empty; // For beneficiary items
        public Dictionary<string, string> ExtraData { get; set; } = new(); // For donor items
    }

    public class CartResponseDto
    {
        public int CartId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
