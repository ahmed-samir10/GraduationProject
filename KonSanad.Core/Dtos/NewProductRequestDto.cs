using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{

    // Request a new product (name, condition/status, reason, optional image URL)
    public class NewProductRequestDto
    {
        public string ProductName { get; set; } = string.Empty;
       // public string Status { get; set; } = string.Empty; // e.g. "New", "Used - Good" الحاله
       public string ProductDescription { get; set; }
       public string Reason { get; set; } = string.Empty; // وصف
        public IFormFile? ImageFile { get; set; }
    }

}
