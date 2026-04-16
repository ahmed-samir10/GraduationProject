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
        public int? Quantity { get; set; }

        public string? ProductName { get; set; }
        public string? imagUrl { get; set; }
        public string? Category { get; set; }
        public string? Type { get; set; }
        public string? Reason { get; set; }

        public Dictionary<string, string>? ExtraData { get; set; }

        // beneficiary supporting files (stored as base64 or saved URLs)
        public List<string>? SupportingFileUrls { get; set; }
    }

    public class CartResponseDto
    {
        public int CartId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
