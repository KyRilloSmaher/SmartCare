using Microsoft.AspNetCore.Identity;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Pharmacist : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateOnly BirthDate { get; set; }
        public string LicenseNumber {  get; set; }
        public bool IsActive { get; set; }
        public string? OTP { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? EmailConfirmationLink { get; set; } = default!;
        public DateTime VerificationURLExpiresAt { get; set; }
        public AccountType AccountType { get; set; }
        public Guid StoreId { get; set; }
        public Store Store { get; set; }
    }
}
