
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
        public DateOnly BirthDate { get; set; }
        public string? OTP { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? EmailConfirmationLink { get; set; } = default!;
        public DateTime VerificationURLExpiresAt { get; set; }
       
        public bool IsDeleted { get; set; } = false;

        public Client? Client { get; set; }
        public Pharmacist? Pharmacist { get; set; }
        public Admin? Admin { get; set; }

    }
}
