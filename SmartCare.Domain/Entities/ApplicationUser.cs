
using Microsoft.AspNetCore.Identity;
using SmartCare.Domain.Enums;


namespace SmartCare.Domain.Entities
{
    public class ApplictionUser: IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public string ProfileImageUrl { get; set; } = string.Empty;
        // Password reset fields
        public string? OTP { get; set; }
        public DateTime? OTPExpiryTime { get; set; }
        public int OTPAttempts { get; set; }
        public bool ResetPasswordConfirmed { get; set; }
        // Refresh Token Feilds
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get;} = DateTime.UtcNow;
        public Client? Client { get; set; }
        public Pharmacist? Pharmacist { get; set; }
        public Admin? Admin { get; set; }
        public Delivery? Delivery { get; set; }

    }
}
