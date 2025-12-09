using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class NewProductDonationDto
    {
        
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; } // جديد، مستعمل، جيد...
      
        public IFormFile ImageFile { get; set; }
    }

}
