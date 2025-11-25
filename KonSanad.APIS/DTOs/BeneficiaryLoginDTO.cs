using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryLoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
