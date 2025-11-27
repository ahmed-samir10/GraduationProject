using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class ExistingProductRequestDto
    {
        public int OrderItemId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}

namespace KonSanad.Core.Dtos
{
    // Request an existing donated item (only reason required)
  

    // Response returned after creating a help request
    public class HelpRequestResponseDto
    {
        public int HelpOrderId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}