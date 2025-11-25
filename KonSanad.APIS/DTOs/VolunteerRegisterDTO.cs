using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class VolunteerRegisterDTO
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

        // معلومات خاصة بالمتطوع
        [Required]
        [Range(18, 100)]
        public int Age { get; set; }

        [MaxLength(100)]
        public string? Occupation { get; set; }

        [MaxLength(200)]
        public string? Skills { get; set; } // مثال: "قيادة، إدارة، طبخ"

        [MaxLength(100)]
        public string? AvailabilityTime { get; set; } // "صباحي، مسائي، عطلات"

        public string? IdentityNumber { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        public bool HasTransportation { get; set; } = false;
    }
}
