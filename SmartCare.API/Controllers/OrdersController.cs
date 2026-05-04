using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Features.Orders.Commands;
using SmartCare.Application.Features.Orders.Commands.CreateOnlineOrder;
using SmartCare.Application.Features.Orders.Commands.CreatePickUpOrder;
using SmartCare.Application.Features.Orders.Commands.DeleteOrder;
using SmartCare.Application.Features.Orders.Commands.UpdateOrder;
using SmartCare.Application.Features.Orders.Commands.UpdateOrderStatus;
using SmartCare.Application.Features.Orders.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;
using System.Security.Claims;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Order with details by Id
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetWithDetailsById)]
        [ProducesResponseType(typeof(Response<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderWithDetailsByIdAsync(Guid id)
        {
            //var result = await _orderService.GetOrderWithDetailsByIdAsync(id);
            var result = await _mediator.Send(new GetOrderWithDetailsByIdAsyncQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get Orders For User
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetForUser)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersForUserAsync()
        {   
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _orderService.GetOrdersByCustomerIdAsync(userId);
            var result = await _mediator.Send(new GetOrdersByCustomerIdAsyncQuery(userId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpGet(ApplicationRouting.Order.GetReadyOfShipOrders)]
        [Authorize(Roles = "DELIVERY")]  
        [ProducesResponseType(typeof(Response<IEnumerable<DeliveryOrderDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetShippingOrdersAsync()
        {
            var result = await _mediator.Send(new GetShippingOrdersQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Confirm Order Delivered — DELIVERY role only
        /// </summary>
        [HttpPatch(ApplicationRouting.Order.ConfirmDelivery)]
        [Authorize(Roles = "DELIVERY")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmDeliveryAsync(Guid orderId)
        {
            var deliveryPersonId = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result = await _mediator.Send(new ConfirmDeliveryCommand(orderId, deliveryPersonId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Accept Delivery Order — DELIVERY role only
        /// Sets order status to DELIVERY_ACCEPTED
        /// </summary>
        [HttpPatch(ApplicationRouting.Order.AcceptDelivery)]
        [Authorize(Roles = "DELIVERY")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AcceptDeliveryAsync(Guid orderId)
        {
            var deliveryPersonId = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var result = await _mediator.Send(new AcceptDeliveryCommand(orderId, deliveryPersonId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Today Online Orders for specific branch — PHARMACIST only
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetTodayOnlineOrders)]
        [Authorize(Roles = "PHARMACIST")]
        [ProducesResponseType(typeof(Response<PaginatedResult<OnlineOrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodayOnlineOrdersAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var storeIdClaim = User.FindFirst("StoreId")?.Value;
            if (string.IsNullOrEmpty(storeIdClaim) || !Guid.TryParse(storeIdClaim, out Guid storeId))
                return Unauthorized("StoreId claim not found.");

            var result = await _mediator.Send(
                new GetTodayOnlineOrdersQuery(storeId, pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Today PickUp Orders for specific branch — PHARMACIST only
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetTodayPickUpOrders)]
        [Authorize(Roles = "PHARMACIST")]
        [ProducesResponseType(typeof(Response<PaginatedResult<PickUpOrderNotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodayPickUpOrdersAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var storeIdClaim = User.FindFirst("StoreId")?.Value;
            if (string.IsNullOrEmpty(storeIdClaim) || !Guid.TryParse(storeIdClaim, out Guid storeId))
                return Unauthorized("StoreId claim not found.");

            var result = await _mediator.Send(
                new GetTodayPickUpOrdersQuery(storeId, pageNumber, pageSize));

            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Update Order Status
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN ,PHARMACIST")]
        [HttpPatch(ApplicationRouting.Order.UpdateStatus)]
        [ProducesResponseType(typeof(Response<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateOrderStatusAsync(Guid id, OrderStatus newStatus)
        {
            var result = await _mediator.Send(new UpdateOrderStatusCommand(id, newStatus));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Verify a pickup code for a given order — PHARMACIST only
        /// </summary>
        [HttpPost(ApplicationRouting.Order.VerifyPickupCode)]
        [Authorize(Roles = "PHARMACIST")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyPickupCode([FromQuery] VerifyPickupOrderRequestDto dto)
        {
            var query = new IsPickupCodeValidAsyncQuery(dto.OrderId, dto.VerifyCode);
            var result = await _mediator.Send(query);
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get Orders by Customer Id
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetByCustomerId)]
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByCustomerIdAsync(string clientId)
        {
            //var result = await _orderService.GetOrdersByCustomerIdAsync(clientId);
            var result = await _mediator.Send(new GetOrdersByCustomerIdAsyncQuery(clientId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Orders Of Client and with Status
        /// </summary>
        [HttpGet(ApplicationRouting.Order.GetOrdersByCustomerAndStatus)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByCustomerIdAsync([FromQuery]string clientId , [FromQuery]OrderStatus status)
        {
            //var result = await _orderService.GetOrdersByCustomerAndStatusAsync(clientId , status);
            var result = await _mediator.Send(new GetOrdersByCustomerAndStatusAsyncQuery(clientId, status));
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
            //var result = await _orderService.GetOrdersByStatus(status, storeId);
            var result = await _mediator.Send(new GetOrdersByStatusQuery(status, storeId));
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
            //var result = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate, storeId);
            var result = await _mediator.Send(new GetOrdersByDateRangeAsyncQuery(startDate, endDate, storeId));
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
            //var result = await _orderService.GetTotalOrdersCountAsync(storeId);
            var result = await _mediator.Send(new GetTotalOrdersCountAsyncQuery(storeId));
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
            //var result = await _orderService.GetTotalRevenueAsync(storeId);
            var result = await _mediator.Send(new GetTotalRevenueAsyncQuery(storeId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Orders with Details
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN , OWNER")]
        [HttpGet(ApplicationRouting.Order.GetAllWithDetails)]
        [ProducesResponseType(typeof(Response<IEnumerable<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersWithDetailsAsync(int pageNumber , int Pagesize)
        {
            var result = await _mediator.Send(new GetOrdersWithDetailsAsyncQuery(pageNumber, Pagesize));
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
            //var result = await _orderService.GetTopNOrdersByValueAsync(n, storeId);
            var result = await _mediator.Send(new GetTopNOrdersByValueAsyncQuery(n, storeId));
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
            //var result = await _orderService.GetRecentOrdersAsync(days, storeId);
            var result = await _mediator.Send(new GetRecentOrdersAsyncQuery(days, storeId));
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
            //var result = await _orderService.GetOrderCountByStatusAsync(storeId);
            var result = await _mediator.Send(new GetOrderCountByStatusAsyncQuery(storeId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Create a new Online Order
        /// </summary>
        [HttpPost(ApplicationRouting.Order.CreateOnline)]
        [ProducesResponseType(typeof(Response<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateOnlineOrderAsync([FromBody] CreateOnlineOrderRequestDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;                                                               
            //var result = await _orderService.CreateOnlineOrderFromCartAsync(userId,dto);
            var result = await _mediator.Send(new CreateOnlineOrderFromCartAsyncCommand(userId, dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Create a new PickUp Order
        /// </summary>
        [HttpPost(ApplicationRouting.Order.CreatePickUp)]
        [ProducesResponseType(typeof(Response<PickUpOrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePickUpOrderAsync([FromBody] CreatePickUpOrderRequestDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _orderService.CreatePickupOrderFromCartAsync(userId,dto);
            var result = await _mediator.Send(new CreatePickupOrderFromCartCommand(userId, dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update Order Status
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN,PHARMACIST")]
        [HttpPatch(ApplicationRouting.Order.UpdateStatus)]
        [ProducesResponseType(typeof(Response<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateOrderStatusAsync(Guid id, OrderStatus newStatus)
        {
            var result = await _mediator.Send(new UpdateOrderStatusCommand(id, newStatus));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update Order Status
        /// </summary>
        [HttpPut(ApplicationRouting.Order.Update)]
        [ProducesResponseType(typeof(Response<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateOrderAsync(UpdateOrderRequestDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _orderService.UpdateOrderAsync(userId, dto);
            var result = await _mediator.Send(new UpdateOrderCommand(userId, dto));
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
            //var result = await _orderService.DeleteOrderAsync(id);
            var result = await _mediator.Send(new DeleteOrderCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
