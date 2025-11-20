    namespace login.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public string? verificationCode { get; set; }
        public DateTime? VerificationCodeExpiresAt { get; set; }

    }
}
