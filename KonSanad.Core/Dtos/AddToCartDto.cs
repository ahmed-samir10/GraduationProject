using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int QuantityRequested {  get; set; }
        public string Reason { get; set; }

        // ✅ New — optional supporting files
        public List<IFormFile>? SupportingFiles { get; set; }
    }
}
