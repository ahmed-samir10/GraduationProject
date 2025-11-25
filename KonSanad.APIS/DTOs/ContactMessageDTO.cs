using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class ContactMessageDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Phone]
        public string Phone { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = null!;

        public string? Subject { get; set; }
    }
}
