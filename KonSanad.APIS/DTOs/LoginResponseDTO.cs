namespace KonSanad.APIS.DTOs
{
    public class LoginResponseDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserType { get; set; } = null!; // "Donor", "Beneficiary", "Volunteer", "Manager"
        public string Token { get; set; } = null!;
        public DateTime TokenExpiration { get; set; }
    }
}
