using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class DonorUpdateDTO
    {
        [MaxLength(200)]
        public string? FullName { get; set; }

        [Phone]
        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }
    }
}
