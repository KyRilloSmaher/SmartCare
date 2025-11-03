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
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(string client);
        Task<IEnumerable<Order>> GetOrdersWithDetailsAsync();
        Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId);
        Task<IEnumerable<Order>> GetOrdersByStatus(OrderStatus status, Guid? storeId = null);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate , Guid? storeId = null);
        Task<IEnumerable<Order>> GetOrdersByCustomerAndStatusAsync(string customerId, OrderStatus status);
        Task<IEnumerable<Order>> GetTopNOrdersByValueAsync(int n , Guid? storeId = null);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int days , Guid? storeId = null);
        Task<Dictionary<OrderStatus, int>> GetOrderCountByStatusAsync(Guid? storeId = null);
        Task<decimal> GetTotalRevenueAsync(Guid? storeId = null);
        Task<int> GetTotalOrdersCountAsync(Guid? storeId = null);


    }
}
