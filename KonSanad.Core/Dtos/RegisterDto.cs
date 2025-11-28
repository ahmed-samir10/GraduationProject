using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace KonSanad.Core.Dtos
{
    public class RegisterDto
    {
        public string DisplayName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;


        public string NationalId { get; set; }

        // Add this to capture user type on registration:
        // Allowed values: "Volunteer", "Beneficiary", "Donor"
        public string Role { get; set; }
    }
}