using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Projection_Models;
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
        Task<Client?> GetByIdWithDetailsAsync(string clientId, bool asTracking = false);
        Task<bool> IsClientPhoneNumberUniqueAsync(string phone);
        Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm);
        Task<ICollection<Guid>> GetClientPurchasesHistoryAsync(string clientId);
        Task<List<ClientPurchaseItem>> GetClientPurchasesHistoryWithDatesAsync(string userId);
    }
}
