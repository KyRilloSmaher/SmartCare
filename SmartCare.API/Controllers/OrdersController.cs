using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get Order by Id
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetById)]
        [ProducesResponseType(typeof(Response<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderByIdAsync(Guid id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Order with details by Id
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetWithDetailsById)]
        [ProducesResponseType(typeof(Response<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderWithDetailsByIdAsync(Guid id)
        {
            var result = await _orderService.GetOrderWithDetailsByIdAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Orders by Customer Id
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetByCustomerId)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByCustomerIdAsync(string clientId)
        {
            var result = await _orderService.GetOrdersByCustomerIdAsync(clientId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Orders Of Client and with Status
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetOrdersByCustomerAndStatus)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByCustomerIdAsync([FromQuery]string clientId , [FromQuery]OrderStatus status)
        {
            var result = await _orderService.GetOrdersByCustomerAndStatusAsync(clientId , status);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Get Orders by Status
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpGet(ApplicationRouting.Order.GetByStatus)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByStatusAsync(OrderStatus status, [FromQuery] Guid? storeId = null)
        {
            var result = await _orderService.GetOrdersByStatus(status, storeId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Orders within a Date Range
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpGet(ApplicationRouting.Order.GetByDateRange)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, [FromQuery] Guid? storeId = null)
        {
            var result = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate, storeId);
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get Orders Counts 
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpGet(ApplicationRouting.Order.GetTotalCount)]
        [ProducesResponseType(typeof(Response<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTotalOrdersCountAsync([FromQuery] Guid ? storeId)
        {
            var result = await _orderService.GetTotalOrdersCountAsync(storeId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Orders Revenue
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpGet(ApplicationRouting.Order.GetTotalRevenue)]
        [ProducesResponseType(typeof(Response<decimal>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTotalRevenueAsync([FromQuery] Guid? storeId)
        {
            var result = await _orderService.GetTotalRevenueAsync(storeId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Orders with Details
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpGet(ApplicationRouting.Order.GetAllWithDetails)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersWithDetailsAsync()
        {
            var result = await _orderService.GetOrdersWithDetailsAsync();
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Top N Orders by Value
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpGet(ApplicationRouting.Order.GetTopNByValue)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopNOrdersByValueAsync(int n, [FromQuery] Guid? storeId = null)
        {
            var result = await _orderService.GetTopNOrdersByValueAsync(n, storeId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Recent Orders
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpGet(ApplicationRouting.Order.GetRecent)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentOrdersAsync(int days, [FromQuery] Guid? storeId = null)
        {
            var result = await _orderService.GetRecentOrdersAsync(days, storeId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Order Count by Status
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpGet(ApplicationRouting.Order.GetCountByStatus)]
        [ProducesResponseType(typeof(Response<Dictionary<OrderStatus, int>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderCountByStatusAsync([FromQuery] Guid? storeId = null)
        {
            var result = await _orderService.GetOrderCountByStatusAsync(storeId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Create a new Order
        /// </summary>
        [HttpPost(ApplicationRouting.Order.Create)]
        [ProducesResponseType(typeof(Response<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderRequestDto dto)
        {
            var result = await _orderService.CreateOrderFromCartAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Update Order Status
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpPatch(ApplicationRouting.Order.UpdateStatus)]
        [ProducesResponseType(typeof(Response<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateOrderStatusAsync(Guid id, OrderStatus newStatus)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, newStatus);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Delete Order
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpDelete(ApplicationRouting.Order.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteOrderAsync(Guid id)
        {
            var result = await _orderService.DeleteOrderAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
