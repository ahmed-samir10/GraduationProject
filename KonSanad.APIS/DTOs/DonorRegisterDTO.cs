using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class DonorRegisterDTO
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        [MaxLength(200)]
        public string Password { get; set; } = null!;

        [Required]
        [Phone]
        [MaxLength(50)]
        public string Phone { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string DonorType { get; set; } = null!; // "Individual" or "Organization"

        // Optional: National ID / Identity Number
        public string? IdentityNumber { get; set; }
    }
}
