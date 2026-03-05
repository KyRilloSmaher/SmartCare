using Microsoft.EntityFrameworkCore;
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
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(string clientId);
        Task<IEnumerable<Order>> GetOrdersWithDetailsAsync();
<<<<<<< HEAD
        Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId , bool track = true);
=======
        Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId , bool asTrack = true);
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
        Task<Order?> GetOrderByPickUpCode(string code);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, Guid? storeId = null);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? storeId = null);
        Task<IEnumerable<Order>> GetOrdersByCustomerAndStatusAsync(string customerId, OrderStatus status);
        Task<IEnumerable<Order>> GetTopNOrdersByValueAsync(int n, Guid? storeId = null);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int days, Guid? storeId = null);
        Task<Dictionary<OrderStatus, int>> GetOrderCountByStatusAsync(Guid? storeId = null);
        Task<decimal> GetTotalRevenueAsync(Guid? storeId = null);
        Task<int> GetTotalOrdersCountAsync(Guid? storeId = null);
        Task<bool> AddOrderItemsAsync(IEnumerable<OrderItem> orderItems);
        Task<IEnumerable<OnlineOrder>> GetOnlineOrdersAsync();
        Task<IEnumerable<PickUpOrder>> GetFromStoreOrdersAsync(Guid? storeId = null);
        Task UpdateOrderItemsAsync(IEnumerable<OrderItem> orderItems);
        Task<OnlineOrder?> GetOnlineOrderAsync(Guid orderId);
        Task<PickUpOrder?> GetOfflineOrderAsync(Guid orderId);
        void RemoveOnlineOrder(OnlineOrder onlineOrder);
        void RemoveOfflineOrder(PickUpOrder offlineOrder);
        Task AddInOnlineOrderAsync(OnlineOrder onlineOrder);
        Task AddInOfflineOrderAsync(PickUpOrder fromStoreOrder);
        Task SwitchOrderTypeAsync( Order order,OrderType newType,Guid? shippingAddressId,Guid? storeId);
        Task UpdatePickupCodeHashAsync(Guid orderId, string pickupCodeHash);
        Task UpdatePaymentIntentIdAsync(Order order , string paymentIntentId);
        Task<OrderStatus> GetOrderStatusDirectAsync(Guid orderId);
    }
}
