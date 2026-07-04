using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class UpdateProfileBenDto
    {
        public string Email { get; set; } = null!;
        
        public string FullName { get; set; }
        public string NationalId { get; set; }
        public string PhoneNumber { get; set; }
        public IFormFile? SupportingFiles { get; set; }
        public string Address { get; set; }
    }
    public class UpdateProfileDto
    {
        public string Email { get; set; } = null!;

        public string FullName { get; set; }
        public string NationalId { get; set; }
        public string PhoneNumber { get; set; }
      //  public IFormFile SupportingFiles { get; set; }
        public string Address { get; set; }
    }

}
