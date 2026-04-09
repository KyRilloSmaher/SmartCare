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

        #region Query Methods

        private IQueryable<Order> BaseOrderQuery(bool trackChanges = false)
        {
            var query = _context.Orders
                .Include(o => (o as OnlineOrder).Address)
                .Include(o => (o as PickUpOrder).Store)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.Payment)
                .Include(o => o.Client)
                .Include(o => o.Client.User);

            return trackChanges ? query.AsTracking() : query.AsNoTracking();
        }

        public IQueryable<Order> GetOrdersQueryable(bool trackChanges = false)
        {
            return BaseOrderQuery(trackChanges);
        }

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

        public async Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId , bool astracked = false)
        {
            return await BaseOrderQuery(astracked)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> GetOrderByPickUpCode(string pickupCodeHash)
        {
            return await _context.Orders
                .OfType<PickUpOrder>()
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
                query = query.OfType<PickUpOrder>()
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
                query = query.OfType<PickUpOrder>()
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
                query = (IOrderedQueryable<Order>)query.OfType<PickUpOrder>()
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
                query = query.OfType<PickUpOrder>()
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
                query = query.OfType<PickUpOrder>()
                             .Where(o => o.StoreId == storeId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(Guid? storeId = null)
        {
            var query = _context.Orders.AsQueryable();

            if (storeId.HasValue)
            {
                query = query.OfType<PickUpOrder>()
                             .Where(o => o.StoreId == storeId.Value);
            }

            return await query.SumAsync(o => o.TotalPrice);
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrderCountByStatusAsync(Guid? storeId = null)
        {
            var query = _context.Orders.AsQueryable();

            if (storeId.HasValue)
            {
                query = query.OfType<PickUpOrder>()
                             .Include(o => o.Store)
                             .Where(o => o.StoreId == storeId.Value);
            }

            var result = await query
                .GroupBy(o => o.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            return result.ToDictionary(x => x.Key, x => x.Count);
        }

        public async Task<IEnumerable<OnlineOrder>> GetOnlineOrdersAsync()
        {
            return await _context.Orders
                .OfType<OnlineOrder>()
                .Include(o => o.Address)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<OnlineOrder?> GetOnlineOrderAsync(Guid orderId)
        {
            return await _context.OnlineOrders
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<PickUpOrder?> GetOfflineOrderAsync(Guid orderId)
        {
            return await _context.FromStoreOrders
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public IQueryable<OnlineOrder> GetTodayOnlineOrdersByStore(Guid storeId)
        {
            var today = DateTime.UtcNow.Date;

            return _context.OnlineOrders
                .Include(o => o.Client)
                    .ThenInclude(c => c.User)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Where(o => o.Items.Any(i => i.Inventory.StoreId == storeId)
                         && o.CreatedAt.Date == today
                         && !o.IsDeleted);
        }

        public async Task<List<PickUpOrder>> GetTodayPickUpOrdersByStoreAsync(Guid storeId, DateTime today)
        {
            return await _context.Orders
                .OfType<PickUpOrder>()
                .Where(o =>
                    o.StoreId == storeId &&
                    o.CreatedAt.Date == today &&
                    !o.IsDeleted &&
                    o.Status != OrderStatus.Cancelled && 
                    o.Status != OrderStatus.Expired &&  
                    o.Status != OrderStatus.Refunded)      
                .Include(o => o.Client)
                    .ThenInclude(c => c.User)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Command Methods

        public override Task DeleteAsync(Order entity)
        {
            entity.IsDeleted = true;
            return UpdateAsync(entity);
        }

        public async Task<bool> AddOrderItemsAsync(IEnumerable<OrderItem> orderItems)
        {
            await _context.OrderItems.AddRangeAsync(orderItems);
            return true;
        }

        public Task UpdateOrderItemsAsync(IEnumerable<OrderItem> orderItems)
        {
            foreach (var item in orderItems)
            {
                _context.Entry(item).Property(x => x.ReservationId).IsModified = true;
            }
            return Task.CompletedTask;
        }

        public async Task SwitchOrderTypeAsync(Order order, OrderType newType, Guid? shippingAddressId, Guid? storeId)
        {
            if (order.OrderType == newType)
                return;

            // Remove old derived row (DB ONLY)
            if (order.OrderType == OrderType.Online)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM OnlineOrders WHERE Id = {0}", order.Id);
            }
            else if (order.OrderType == OrderType.InStore)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM FromStoreOrders WHERE Id = {0}", order.Id);
            }

            // Insert new derived row
            if (newType == OrderType.Online)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO OnlineOrders (Id, ShippingAddressId) VALUES ({0}, {1})",
                    order.Id, shippingAddressId);
            }
            else
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO FromStoreOrders (Id, StoreId) VALUES ({0}, {1})",
                    order.Id, storeId);
            }

            // Update base discriminator
            order.OrderType = newType;
        }

        public async Task UpdatePickupCodeHashAsync(Guid orderId, string pickupCodeHash)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE FromStoreOrders SET PickupCodeHash = {0} WHERE Id = {1}",
                pickupCodeHash, orderId);
        }

        public Task RemoveOnlineOrder(OnlineOrder onlineOrder)
        {
            _context.OnlineOrders.Remove(onlineOrder);
            return Task.CompletedTask;
        }

        public Task RemoveOfflineOrder(PickUpOrder offlineOrder)
        {
            _context.FromStoreOrders.Remove(offlineOrder);
            return Task.CompletedTask;
        }

        public async Task AddInOnlineOrderAsync(OnlineOrder onlineOrder)
        {
            await _context.OnlineOrders.AddAsync(onlineOrder);
        }

        public async Task AddInOfflineOrderAsync(PickUpOrder PickUpOrder)
        {
            await _context.FromStoreOrders.AddAsync(PickUpOrder);
        }



        void IOrderRepository.RemoveOnlineOrder(OnlineOrder onlineOrder)
        {
            _context.OnlineOrders.Remove(onlineOrder);

        }

        void IOrderRepository.RemoveOfflineOrder(PickUpOrder offlineOrder)
        {
            _context.FromStoreOrders.Remove(offlineOrder);

        }

        public async Task<IEnumerable<PickUpOrder>> GetFromStoreOrdersAsync(Guid? storeId = null)
        {
            var query = _context.Orders
                .OfType<PickUpOrder>()
                .Include(o => o.Store)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .AsQueryable();

            if (storeId.HasValue)
                query = query.Where(o => o.StoreId == storeId.Value);

            return await query.AsNoTracking().ToListAsync();
        }


        #endregion
    }
}