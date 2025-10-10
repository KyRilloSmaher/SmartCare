

using Microsoft.AspNetCore.Identity;

namespace SmartCare.Domain.Entities
{
 public class UserRefreshToken : IdentityUserToken<string>
    {
        // Single PK to simplify queries
        public int Id { get; set; }
        public string? RefreshToken { get; set; }
        public string? JwtId { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime AddedTime { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

}
