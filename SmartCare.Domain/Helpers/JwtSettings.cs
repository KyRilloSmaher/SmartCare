using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Helpers
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty!;
        public string Issuer { get; set; } = string.Empty!;
        public string Audience { get; set; } = string.Empty!;
        public bool ValidateIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateLifeTime { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public int AccessTokenLifetimeHours { get; set; }
        public int RefreshTokenLifetimeDays { get; set; }
    }

}
