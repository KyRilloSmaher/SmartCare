using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;
using SmartCare.InfraStructure.DbContexts;
using System.Collections.Immutable;
namespace SmartCare.InfraStructure.Repositories
{
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        #region Fields
        private readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public ClientRepository(ApplicationDBContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        #endregion

        #region Methods
        public async Task<Client?> GetByIdAsync(string clientId, bool asTracking = false)
        {
            return await _context.Clients
                                .FirstOrDefaultAsync(u => u.Id == clientId);
        }
        public async Task<Client?> GetByIdWithDetailsAsync(string clientId, bool trackChanges = false)
        {
            var query = _context.Clients
                .Include(u => u.Addresses)
                .Include(u => u.User)
                .Where(u => u.Id == clientId);

            return trackChanges
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<Client>> GetAllAsync(bool asTracking = false)
        {
            var query = _context.Clients
                .Include(u => u.Addresses)
                .Include(u => u.User);

            return asTracking
                ? await query.ToListAsync()
                : await query.AsNoTracking().ToListAsync();
        }

        public async Task<bool> IsClientPhoneNumberUniqueAsync(string phone)
        {
            return !await _context.Users.AnyAsync(u => u.PhoneNumber == phone);
        }

        public async Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm)
        {
            return await _context.Clients
                .Include(c => c.User)
                .Where(u =>
                    u.User.Email.Contains(searchTerm) ||
                    u.User.UserName.Contains(searchTerm))
                .AsNoTracking()
                .ToListAsync();
        }

        public IQueryable<Client> GetClientsQueryable(bool trackChanges = false)
        {
            var query = _context.Clients
                .Include(u => u.Addresses)
                .Include(u => u.User);

            return trackChanges ? query : query.AsNoTracking();
        }

        public async Task<ICollection<Guid>> GetClientPurchasesHistoryAsync(string clientId)
        {
            var ordersByClient = _context.Orders.Where(u => u.ClientId == clientId).Select(o=>o.Id);
            var productIds = _context.OrderItems.Where(oi=>ordersByClient.Contains(oi.OrderId)).Select(oi=>oi.ProductId);
            return productIds.ToImmutableList();
        }
        public async Task<List<ClientPurchaseItem>> GetClientPurchasesHistoryWithDatesAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.ClientId == userId)
                .SelectMany(o => o.Items)
                .Select(oi => new ClientPurchaseItem
                {
                    ProductId = oi.ProductId,
                    PurchaseDate = oi.Order.CreatedAt
                })
                .Distinct()
                .ToListAsync();
        }
        
        #endregion
    }
}