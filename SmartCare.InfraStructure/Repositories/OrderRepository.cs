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
                                                .ToListAsync();
            return orders;
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await _context.Orders.Where(o => o.Status == status)
                                                .Include(o => o.Items)
                                                  .ThenInclude(oi => oi.Product)
                                                    .ThenInclude(p => p.Images)
                                                .ToListAsync();
            return orders;
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
        #endregion
    }
}
