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

<<<<<<< HEAD
        #region === Base Query Helper ===
        private IQueryable<Order> BaseOrderQuery(bool tracking = true)
        {
            var query = _dbContext.Orders
=======
        #region Query Methods

        private IQueryable<Order> BaseOrderQuery(bool trackChanges = false)
        {
            var query = _context.Orders
                .Include(o => (o as OnlineOrder).Address)
                .Include(o => (o as PickUpOrder).Store)
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
                .Include(o => o.Items)
                .ThenInclude(i=>i.Product)
                .ThenInclude(p => p.Images)
                .Include(o => o.Payment)
                .Include(o => o.Client)
<<<<<<< HEAD
                .OrderByDescending(o=>o.CreatedAt)
                .AsQueryable();

            if (!tracking)
                query = query.AsNoTracking();

            return query;
        }


        #endregion
=======
                .Include(o => o.Client.User);
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8

            return trackChanges ? query : query.AsNoTracking();
        }

        public IQueryable<Order> GetOrdersQueryable(bool trackChanges = false)
        {
            return BaseOrderQuery(trackChanges);
        }

        public async Task<Order?> GetOrderWithDetailsByIdAsync(Guid id, bool tracking = true)
        {
            var query = BaseOrderQuery(tracking);

            var order = await query.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return null;

            if (order is OnlineOrder online)
            {
                await _dbContext.Entry(online)
                    .Reference(o => o.Address)
                    .LoadAsync();
            }
            else if (order is FromStoreOrder store)
            {
                await _dbContext.Entry(store)
                    .Reference(o => o.Store)
                    .LoadAsync();
            }

            return order;
        }


        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(string clientId)
        {
            var onlineOrders = await BaseOrderQuery()
                .Where(o => o.ClientId == clientId)
                .OfType<OnlineOrder>()
                .Include(o => o.Address)
                .ToListAsync();

            var storeOrders = await BaseOrderQuery()
                .Where(o => o.ClientId == clientId)
                .OfType<FromStoreOrder>()
                .Include(o => o.Store)
                .ToListAsync();

            return onlineOrders
                .Cast<Order>()
                .Concat(storeOrders);
        }


<<<<<<< HEAD


        public override async Task<bool> DeleteAsync(Order entity)
        {
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
=======
        public async Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId, bool asTrack = true)
        {
            return await BaseOrderQuery(asTrack)
                .FirstOrDefaultAsync(o => o.Id == orderId);
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
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

<<<<<<< HEAD
            return await query.ToListAsync();
        }
        public async Task UpdateOrderItemsAsync(IEnumerable<OrderItem> orderItems)
        {
            foreach (var item in orderItems)
            {
                _dbContext.OrderItems.Attach(item);

                _dbContext.Entry(item).Property(x => x.ReservationId).IsModified = true;
            }

            await _dbContext.SaveChangesAsync();
=======
            return await query.AsNoTracking().ToListAsync();
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
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

<<<<<<< HEAD
        public async Task AddInOfflineOrderAsync(FromStoreOrder fromStoreOrder)
        {
            await _dbContext.FromStoreOrders.AddAsync(fromStoreOrder);
        }
=======
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
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
        //public async Task SwitchOrderTypeAsync(Order order, OrderType newType, Guid? shippingAddressId, Guid? storeId)
        //{
        //    if (order.OrderType == newType)
        //        return;

        //    // Detach old derived entity (if tracked) to avoid EF conflicts
        //    if (_dbContext.Entry(order).State == EntityState.Detached)
        //        _dbContext.Attach(order);

        //    // Remove old derived row
        //    if (order.OrderType == OrderType.Online)
        //    {
        //        var oldOnline = await _dbContext.OnlineOrders
        //            .FirstOrDefaultAsync(o => o.Id == order.Id);
        //        if (oldOnline != null)
        //            _dbContext.OnlineOrders.Remove(oldOnline);
        //    }
        //    else if (order.OrderType == OrderType.InStore)
        //    {
        //        var oldStore = await _dbContext.FromStoreOrders
        //            .FirstOrDefaultAsync(o => o.Id == order.Id);
        //        if (oldStore != null)
        //            _dbContext.FromStoreOrders.Remove(oldStore);
        //    }

        //    // Insert new derived row
        //    if (newType == OrderType.Online)
        //    {
        //        var onlineOrder = new OnlineOrder
        //        {
        //            Id = order.Id,
        //            ShippingAddressId = shippingAddressId!.Value
        //        };
        //        await _dbContext.OnlineOrders.AddAsync(onlineOrder);
        //    }
        //    else if (newType == OrderType.InStore)
        //    {
        //        var fromStoreOrder = new FromStoreOrder
        //        {
        //            Id = order.Id,
        //            StoreId = storeId!.Value
        //        };
        //        await _dbContext.FromStoreOrders.AddAsync(fromStoreOrder);
        //    }

        //    // Update base entity discriminator
        //    order.OrderType = newType;

        //    // Save all changes in a single transaction
        //    await _dbContext.SaveChangesAsync();
        //}

        //public async Task UpdatePickupCodeHashAsync(Guid orderId, string pickupCodeHash)
        //{
        //    await _dbContext.Database.ExecuteSqlRawAsync("UPDATE FromStoreOrders SET PickupCodeHash = {0} WHERE Id = {1}",pickupCodeHash,orderId);
        //}


        //public async Task UpdatePaymentIntentIdAsync(Guid orderId, string paymentIntentId)
        //{
        //    await _dbContext.Database.ExecuteSqlRawAsync(
        //        "UPDATE [Order] SET PaymentIntentId = {0} WHERE Id = {1}",
        //        paymentIntentId,
        //        orderId);
        //}

        //public async Task<OrderStatus> GetOrderStatusDirectAsync(Guid orderId)
        //{
        //    // Use AsNoTracking to bypass change tracking but still use EF
        //    var order = await _dbContext.Orders
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(o => o.Id == orderId);

        //    var status = order.Status;
        //    if (Enum.IsDefined(typeof(OrderStatus), status))
        //    {
        //        return status;
        //    }

        //    throw new InvalidOperationException($"Order {orderId} not found");
        //}

        public async Task UpdatePaymentIntentIdAsync(Order order, string paymentIntentId)
        {
            var entry = _dbContext.Entry(order);
            order.PaymentIntentId = paymentIntentId;

            await _dbContext.SaveChangesAsync();
        }


        public async Task UpdatePickupCodeHashAsync(Guid orderId, string pickupCodeHash)
        {
<<<<<<< HEAD
            var order = await _context.Orders
                .OfType<FromStoreOrder>()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                order.PickupCodeHash = pickupCodeHash;
                // Note: Caller should call SaveChangesAsync()
            }
            else
            {
                throw new InvalidOperationException($"FromStoreOrder {orderId} not found");
            }
        }

        public async Task<OrderStatus> GetOrderStatusDirectAsync(Guid orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null && Enum.IsDefined(typeof(OrderStatus), order.Status))
            {
                return order.Status;
            }

            throw new InvalidOperationException($"Order {orderId} not found");
        }


        public async Task<IEnumerable<Order>> GetOrdersWithDetailsAsync() =>  BaseOrderQuery();

=======
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE FromStoreOrders SET PickupCodeHash = {0} WHERE Id = {1}",
                pickupCodeHash, orderId);
        }

        public Task RemoveOnlineOrder(OnlineOrder onlineOrder)
        {
            _context.OnlineOrders.Remove(onlineOrder);
            return Task.CompletedTask;
        }
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8

        public Task RemoveOfflineOrder(PickUpOrder offlineOrder)
        {
            _context.FromStoreOrders.Remove(offlineOrder);
            return Task.CompletedTask;
        }

        public async Task AddInOnlineOrderAsync(OnlineOrder onlineOrder)
        {
            await _context.OnlineOrders.AddAsync(onlineOrder);
        }

        public async Task AddInOfflineOrderAsync(PickUpOrder fromStoreOrder)
        {
            await _context.FromStoreOrders.AddAsync(fromStoreOrder);
        }

        

        void IOrderRepository.RemoveOnlineOrder(OnlineOrder onlineOrder)
        {
            _context.OnlineOrders.Remove(onlineOrder);

        }

        void IOrderRepository.RemoveOfflineOrder(PickUpOrder offlineOrder)
        {
            _context.FromStoreOrders.Remove(offlineOrder);

        }


        #endregion
    }
}