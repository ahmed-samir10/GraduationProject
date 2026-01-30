using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace KonSanad.Core.Dtos
{
    public class RegisterDto
    {
        public string FullName { get; set; }
   

        public string Email { get; set; }
        public string Password { get; set; }

        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;

        public string? Skills { get; set; } 
        public string NationalId { get; set; }

        // Add this to capture user type on registration:
        // Allowed values: "Volunteer", "Beneficiary", "Donor"
        public string Role { get; set; }
    }
}