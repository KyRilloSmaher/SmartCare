using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Handlers.ClaimsHandler
{
    public class PharmacistClaimsProvider : IClaimsProvider
    {
        public Task<IEnumerable<Claim>> GetClaimsAsync(ApplictionUser user)
        {
            var claims = new List<Claim>();

            if (user.Pharmacist != null)
            {
                claims.Add(new Claim("StoreId", user.Pharmacist.StoreId.ToString()));
                claims.Add(new Claim("LicenseNumber", user.Pharmacist.LicenseNumber ?? string.Empty));
                claims.Add(new Claim("IsActive", user.Pharmacist.IsActive.ToString()));
            }

            return Task.FromResult<IEnumerable<Claim>>(claims);
        }
    }
}
