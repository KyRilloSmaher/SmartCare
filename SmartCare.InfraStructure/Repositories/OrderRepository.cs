using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class OrderRepository : GenericRepository<Order> ,IOrderRepository
    {
        #region Feilds
        public readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public OrderRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }


        #endregion

        #region Methods

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(string customerId)
        {
            var orders =  await _context.Orders.Where(o => o.ClientId == customerId)
                                               .Include(o => o.Items)
                                                  .ThenInclude(oi => oi.Product)
                                                    .ThenInclude(p => p.Images)
                                               .Include(o => o.Address)
                                               .ToListAsync();
            return orders;
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate,DateTime endDate,Guid? storeId = null)
        {
            var query = _context.Orders
                                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                                .Include(o => o.Items)
                                    .ThenInclude(oi => oi.Product)
                                        .ThenInclude(p => p.Images)
                                .AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }

            return await query.ToListAsync();
        }


        public async Task<IEnumerable<Order>> GetOrdersByStatus(OrderStatus status , Guid? storeId = null)
        {
            var query = _context.Orders
                     .Where(o => o.Status == status)
                     .Include(o => o.Items)
                         .ThenInclude(oi => oi.Product)
                             .ThenInclude(p => p.Images)
                     .AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersWithDetailsAsync()
        {
            var orders = await _context.Orders
                                        .Include(o => o.Items)
                                          .ThenInclude(oi => oi.Product)
                                            .ThenInclude(p => p.Images)
                                        .ToListAsync();
            return orders;

        }

        public async Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId)
        {
            return await  _context.Orders
                            .Include(o => o.Items)
                              .ThenInclude(oi => oi.Product)
                                .ThenInclude(p => p.Images)
                            .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int days, Guid? storeId = null)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var query = _context.Orders
                    .Where(o => o.CreatedAt >= cutoffDate)
                    .Include(o => o.Items)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Images)
                    .AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetTopNOrdersByValueAsync(int n, Guid? storeId = null)
        {
            var query = _context.Orders
                                    .OrderByDescending(o => o.TotalPrice)
                                    .Take(n)
                                    .Include(o => o.Items)
                                      .ThenInclude(oi => oi.Product)
                                          .ThenInclude(p => p.Images)
                                    .AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }

            return await query.ToListAsync();        

        }

        public async Task<int> GetTotalOrdersCountAsync(Guid? storeId = null)
        {
            var query = _context.Orders.AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(Guid? storeId = null)
        {
            var query = _context.Orders.AsQueryable();
            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }
            return await query.SumAsync(o => o.TotalPrice);
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrderCountByStatusAsync(Guid? storeId = null)
        {
            var query = _context.Orders.AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }

            var result = await query
                                .GroupBy(o => o.Status)
                                .Select(g => new { Status = g.Key, Count = g.Count() })
                                .ToListAsync();

            return result.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAndStatusAsync(string customerId, OrderStatus status)
        {
            var orders = await _context.Orders
                                        .Where(o => o.ClientId == customerId && o.Status == status)
                                        .Include(o => o.Items)
                                          .ThenInclude(oi => oi.Product)
                                            .ThenInclude(p => p.Images)
                                        .ToListAsync();
            return orders;
        }
        #endregion
    }
}
