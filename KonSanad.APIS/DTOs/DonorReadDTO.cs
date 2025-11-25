namespace KonSanad.APIS.DTOs
{
    public class DonorReadDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string DonorType { get; set; } = null!;

        // Statistics
        public int TotalDonations { get; set; }
        public decimal TotalCashDonated { get; set; }
        public int TotalItemsDonated { get; set; }
    }
}
