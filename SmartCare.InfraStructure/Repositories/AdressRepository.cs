using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class AdressRepository : GenericRepository<Address>, IAddressRepository
    {
        private readonly ApplicationDBContext _context;

        public AdressRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;

        }

        public async Task<Address?>  GetClientAddressByIdAsync(string clientId, Guid addressId)
        {
            return await _context.Addresses.AsNoTracking()
                .FirstOrDefaultAsync(a => a.ClientId == clientId && a.Id == addressId);
        }

        public async Task<IEnumerable<Address>> GetClientAddressesAsync(string clientId)
        {
            return await _context.Addresses.AsNoTracking()
                .Where(a => a.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<Address?> GetPrimaryAddressAsync(string clientId)
        {
            return await _context.Addresses.AsTracking()
                .FirstOrDefaultAsync(a => a.ClientId == clientId && a.IsPrimary);
        }
    }
}
