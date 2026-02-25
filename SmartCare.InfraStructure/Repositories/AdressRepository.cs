using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        private readonly ApplicationDBContext _context;

        public AddressRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Address> GetAddressesQueryable(string? clientId = null, bool trackChanges = false)
        {
            var query = _context.Addresses.AsQueryable();

            if (!string.IsNullOrEmpty(clientId))
                query = query.Where(a => a.ClientId == clientId);

            return trackChanges ? query : query.AsNoTracking();
        }

        public async Task<Address?> GetClientAddressByIdAsync(string clientId, Guid addressId, bool trackChanges = false)
        {
            var query = _context.Addresses
                .Where(a => a.ClientId == clientId && a.Id == addressId);

            return trackChanges
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Address>> GetClientAddressesAsync(string clientId)
        {
            return await _context.Addresses
                .Where(a => a.ClientId == clientId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Address?> GetPrimaryAddressAsync(string clientId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.ClientId == clientId && a.IsPrimary);
        }

        public async Task<bool> HasPrimaryAddressAsync(string clientId)
        {
            return await _context.Addresses
                .AnyAsync(a => a.ClientId == clientId && a.IsPrimary);
        }

        public async Task ClearPrimaryFlagAsync(string clientId, Guid? excludeAddressId = null)
        {
            var primaryAddresses = await _context.Addresses
                .Where(a => a.ClientId == clientId && a.IsPrimary)
                .ToListAsync();

            foreach (var address in primaryAddresses)
            {
                if (excludeAddressId == null || address.Id != excludeAddressId)
                    address.IsPrimary = false;
            }
        }
    }
}