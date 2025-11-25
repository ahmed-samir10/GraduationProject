using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class VolunteerUpdateDTO
    {
        [MaxLength(200)]
        public string? FullName { get; set; }

        [Phone]
        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Occupation { get; set; }

        [MaxLength(200)]
        public string? Skills { get; set; }

        [MaxLength(100)]
        public string? AvailabilityTime { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        public bool? HasTransportation { get; set; }
    }
}
