using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.InfraStructure.Repositories;

namespace SmartCare.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDBContext _context;

        public OrderRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        #region === Base Query Helper ===
        private IQueryable<Order> BaseOrderQuery()
        {
            return _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.Payment)
                .Include(o => o.Client);
        }
        #endregion

        #region Methods

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(string customerId)
        {
            return await BaseOrderQuery()
                .Where(o => o.ClientId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersWithDetailsAsync()
        {
            return await BaseOrderQuery().ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId)
        {
            return await BaseOrderQuery()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, Guid? storeId = null)
        {
            var query = BaseOrderQuery().Where(o => o.Status == status);

            if (storeId.HasValue)
            {
                query = query.OfType<FromStoreOrder>()
                             .Where(o => o.StoreId == storeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? storeId = null)
        {
            var query = BaseOrderQuery()
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);

            if (storeId.HasValue)
            {
                query = query.OfType<FromStoreOrder>()
                             .Where(o => o.StoreId == storeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAndStatusAsync(string customerId, OrderStatus status)
        {
            return await BaseOrderQuery()
                .Where(o => o.ClientId == customerId && o.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetTopNOrdersByValueAsync(int n, Guid? storeId = null)
        {
            var query = BaseOrderQuery().OrderByDescending(o => o.TotalPrice);

            if (storeId.HasValue)
            {
                query = (IOrderedQueryable<Order>)query.OfType<FromStoreOrder>()
                             .Where(o => o.StoreId == storeId.Value);
            }

            return await query.Take(n).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int days, Guid? storeId = null)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var query = BaseOrderQuery().Where(o => o.CreatedAt >= cutoffDate);

            if (storeId.HasValue)
            {
                query = query.OfType<FromStoreOrder>()
                             .Where(o => o.StoreId == storeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> GetTotalOrdersCountAsync(Guid? storeId = null)
        {
            var query = _context.Orders.AsQueryable();

            if (storeId.HasValue)
            {
                query = query.OfType<FromStoreOrder>()
                             .Where(o => o.StoreId == storeId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(Guid? storeId = null)
        {
            var query = _context.Orders.AsQueryable();

            if (storeId.HasValue)
            {
                query = query.OfType<FromStoreOrder>()
                             .Where(o => o.StoreId == storeId.Value);
            }

            return await query.SumAsync(o => o.TotalPrice);
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrderCountByStatusAsync(Guid? storeId = null)
        {
            var query = _context.Orders.AsQueryable();

            if (storeId.HasValue)
            {
                query = query.OfType<FromStoreOrder>()
                             .Where(o => o.StoreId == storeId.Value);
            }

            var result = await query.GroupBy(o => o.Status)
                                    .Select(g => new { g.Key, Count = g.Count() })
                                    .ToListAsync();

            return result.ToDictionary(x => x.Key, x => x.Count);
        }

        public async Task<bool> AddOrderItemsAsync(IEnumerable<OrderItem> orderItems)
        {
            await _context.OrderItems.AddRangeAsync(orderItems);
            return true;
        }


        public async Task<IEnumerable<OnlineOrder>> GetOnlineOrdersAsync()
        {
            return await _context.Orders
                .OfType<OnlineOrder>()
                .Include(o => o.Address)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .ToListAsync();
        }

        public async Task<IEnumerable<FromStoreOrder>> GetFromStoreOrdersAsync(Guid? storeId = null)
        {
            var query = _context.Orders
                .OfType<FromStoreOrder>()
                .Include(o => o.Store)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }

            return await query.ToListAsync();
        }

        #endregion
    }
}
