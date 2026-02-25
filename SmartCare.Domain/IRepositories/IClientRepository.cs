using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IClientRepository : IGenericRepository<Client>
    {

        Task<Client?> GetByIdAsync(string clientId, bool asTracking = false);
        Task<bool> IsClientPhoneNumberUniqueAsync(string phone);

        Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm);
    }
}
