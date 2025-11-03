using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Order>> GetOrdersWithDetailsAsync();
        Task<Order> GetOrderWithDetailsByIdAsync(Guid orderId);
        Task<IEnumerable<Order>> GetOrdersByStatus(OrderStatus status);

    }
}
