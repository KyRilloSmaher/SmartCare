using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Handlers.ClaimsHandler
{
    public interface IClaimsProvider
    {
        Task<IEnumerable<Claim>> GetClaimsAsync(ApplictionUser user);
    }
}
