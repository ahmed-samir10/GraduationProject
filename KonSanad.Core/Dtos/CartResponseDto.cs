using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class CartResponseDto
    {
        public int CartId { get; set; }
        public List<CartItemResponseDto> Items { get; set; }
       
    }
}
