using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryRegisterDTO
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

        // معلومات خاصة بالمستفيد
        [Required]
        [MaxLength(50)]
        public string BeneficiaryType { get; set; } = null!; // "Individual", "Family", "Institution"

        [MaxLength(100)]
        public string? CaseType { get; set; } // "Medical", "Educational", "Housing", "Food", "Other"

        public string? IdentityNumber { get; set; }

        [Range(0, int.MaxValue)]
        public int? FamilyMembersCount { get; set; }

        [MaxLength(1000)]
        public string? CaseDescription { get; set; }
    }
}
