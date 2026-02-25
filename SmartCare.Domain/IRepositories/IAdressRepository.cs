using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IAddressRepository : IGenericRepository<Address>
    {
        Task<IEnumerable<Address>> GetClientAddressesAsync(string clientId);
        Task<Address?> GetClientAddressByIdAsync(string clientId, Guid addressId, bool trackChanges = false);
        Task<Address?> GetPrimaryAddressAsync(string clientId);
    }
}
