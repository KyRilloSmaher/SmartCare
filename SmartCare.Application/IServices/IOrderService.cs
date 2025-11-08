
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;


namespace SmartCare.Application.IServices
{
    public interface IOrderService
    {
        Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerIdAsync(string clientId);
        Task<Response<OrderResponseDto?>> GetOrderWithDetailsByIdAsync(Guid orderId);
        Task<Response<OrderResponseDto>> GetOrderByIdAsync(Guid orderId);

        Task<Response<int>> GetTotalOrdersCountAsync(Guid? storeId = null);
        Task<Response<decimal>> GetTotalRevenueAsync(Guid? storeId = null);

        Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByStatus(OrderStatus status, Guid? storeId = null);
        Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersWithDetailsAsync();
        Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? storeId = null);
        Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerAndStatusAsync(string customerId, OrderStatus status);
        Task<Response<IEnumerable<OrderResponseDto>>> GetTopNOrdersByValueAsync(int n, Guid? storeId = null);
        Task<Response<IEnumerable<OrderResponseDto>>> GetRecentOrdersAsync(int days, Guid? storeId = null);

        Task<Response<Dictionary<OrderStatus, int>>> GetOrderCountByStatusAsync(Guid? storeId = null);

        Task<Response<OrderResponseDto>> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<Response<OrderResponseDto?>> CreateOrderFromCartAsync(CreateOrderRequestDto dto);
        Task<Response<bool>> DeleteOrderAsync(Guid orderId);
    }

}
