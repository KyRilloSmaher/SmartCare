using Azure;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IOrderService
    {
        Task<Response<OrderResponseDto>> GetOrdersByCustomerIdAsync(string clientId);
        Task<Response<OrderResponseDto?>> GetOrderWithDetailsByIdAsync(Guid orderId);
        Task<Response<OrderResponseDto>> GetOrderByIdAsync(Guid orderId);

        Task<Response<int>> GetTotalOrdersCountAsync(Guid? storeId = null);
        Task<Response<decimal>> GetTotalRevenueAsync();

        Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByStatus(OrderStatus status , Guid? storeId = null);
        Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersWithDetailsAsync();
        Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate , Guid? storeId = null);
        Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerAndStatusAsync(string customerId, OrderStatus status);
        Task<Response<IEnumerable<OrderResponseDto>>> GetTopNOrdersByValueAsync(int n , Guid? storeId = null);
        Task<Response<IEnumerable<OrderResponseDto>>> GetRecentOrdersAsync(int days , Guid? storeId = null);

        Task<Response<Dictionary<OrderStatus, int>>> GetOrderCountByStatusAsync(Guid? storeId = null);

        Task<Response<OrderResponseDto>> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<Response<OrderResponseDto>> CreateOrderAsync(CreateOrderRequestDto dto);
        Task<Response<OrderResponseDto>> UpdateOrderAsync(UpdateOrderRequestDto orderDto);
        Task<Response<bool>> DeleteOrderAsync(Guid orderId);

    }
}
