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
                .Include(o => (o as OnlineOrder).Address)
                .Include(o => (o as FromStoreOrder).Store)
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
        public async Task<Order?> GetOrderByPickUpCode(string pickupCodeHash)
        {
            return await _context.Orders
                                    .OfType<FromStoreOrder>()
                                    .Include(o => o.Store)
                                    .Include(o => o.Items)
                                        .ThenInclude(i => i.Product)
                                            .ThenInclude(p => p.Images)
                                    .Include(o => o.Payment)
                                    .Include(o => o.Client)
                                    .FirstOrDefaultAsync(o => o.PickupCodeHash == pickupCodeHash);
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
                             .Include(o => o.Store)
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
                              .Include(o => o.Store)
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

            return await _context.SaveChangesAsync() > 0;
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
        public async override Task<bool> DeleteAsync(Order entity)
        {
            entity.IsDeleted = true;
            _dbContext.Orders.Update(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task UpdateOrderItemsAsync(IEnumerable<OrderItem> orderItems)
        {
            foreach (var item in orderItems)
            {
                _dbContext.OrderItems.Attach(item);

                _dbContext.Entry(item).Property(x => x.ReservationId).IsModified = true;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<OnlineOrder?> GetOnlineOrderAsync(Guid orderId)
        {
            return await _dbContext.OnlineOrders
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<FromStoreOrder?> GetOfflineOrderAsync(Guid orderId)
        {
            return await _dbContext.FromStoreOrders
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public void RemoveOnlineOrder(OnlineOrder onlineOrder)
        {
            _dbContext.OnlineOrders.Remove(onlineOrder);
        }

        public void RemoveOfflineOrder(FromStoreOrder offlineOrder)
        {
            _dbContext.FromStoreOrders.Remove(offlineOrder);
        }

        public async Task AddInOnlineOrderAsync(OnlineOrder onlineOrder)
        {
            await _dbContext.OnlineOrders.AddAsync(onlineOrder);
        }

        public async Task AddInOfflineOrderAsync(FromStoreOrder fromStoreOrder)
        {
            await _dbContext.FromStoreOrders.AddAsync(fromStoreOrder);
        }
        public async Task SwitchOrderTypeAsync(Order order,OrderType newType,Guid? shippingAddressId,Guid? storeId)
        {
            if (order.OrderType == newType)
                return;

            // Remove old derived row (DB ONLY)
            if (order.OrderType == OrderType.Online)
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "DELETE FROM OnlineOrders WHERE Id = {0}", order.Id);
            }
            else if (order.OrderType == OrderType.InStore)
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "DELETE FROM FromStoreOrders WHERE Id = {0}", order.Id);
            }

            // Insert new derived row
            if (newType == OrderType.Online)
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO OnlineOrders (Id, ShippingAddressId) VALUES ({0}, {1})",
                    order.Id, shippingAddressId);
            }
            else
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO FromStoreOrders (Id, StoreId) VALUES ({0}, {1})",
                    order.Id, storeId);
            }

            // Update base discriminator
            order.OrderType = newType;
            //_dbContext.Orders.Update(order);
        }

        public async Task UpdatePickupCodeHashAsync(Guid orderId, string pickupCodeHash)
        {
            await _dbContext.Database.ExecuteSqlRawAsync("UPDATE FromStoreOrders SET PickupCodeHash = {0} WHERE Id = {1}",pickupCodeHash,orderId);
        }




        #endregion
    }
}
