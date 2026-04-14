using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Handlers.RoleHandler
{
    public interface IRoleLoginHandler
    {
        string Role { get; }
        Task HandleAsync(ApplictionUser user);
    }
}
