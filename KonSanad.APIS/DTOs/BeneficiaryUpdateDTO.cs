using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class BeneficiaryUpdateDTO
    {
        [MaxLength(200)]
        public string? FullName { get; set; }

        [Phone]
        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? CaseType { get; set; }

        [Range(0, int.MaxValue)]
        public int? FamilyMembersCount { get; set; }

        [MaxLength(1000)]
        public string? CaseDescription { get; set; }
    }
}
