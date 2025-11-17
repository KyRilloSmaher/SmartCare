using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartCare.Application.Services
{
    public class OrderService : IOrderService
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        #endregion

        #region Constructor
        public OrderService(
            IResponseHandler responseHandler,
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IReservationRepository reservationRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IStoreRepository storeRepository,
            IPaymentService paymentService,
            IBackgroundJobService backgroundJobService,
            IClientRepository clientRepository,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            _responseHandler = responseHandler ?? throw new ArgumentNullException(nameof(responseHandler));
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _reservation_repository_guard(reservationRepository);
            _reservationRepository = reservationRepository;
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            void _reservation_repository_guard(IReservationRepository r)
            {
                if (r == null) throw new ArgumentNullException(nameof(reservationRepository));
            }
        }
        #endregion

        #region Public API (Interface Implementation)

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerIdAsync(string clientId)
        {
            _logger.LogDebug("GetOrdersByCustomerIdAsync called for ClientId={ClientId}", clientId);

            if (string.IsNullOrWhiteSpace(clientId))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.BAD_REQUEST);

            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
            {
                _logger.LogWarning("Client not found for ClientId={ClientId}", clientId);
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.USER_NOT_FOUND);
            }

            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(clientId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            _logger.LogInformation("Retrieved {Count} orders for client {ClientId}", orders.Count(), clientId);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<OrderResponseDto?>> GetOrderWithDetailsByIdAsync(Guid orderId)
        {
            _logger.LogDebug("GetOrderWithDetailsByIdAsync called for OrderId={OrderId}", orderId);

            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.BAD_REQUEST);

            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found OrderId={OrderId}", orderId);
                return _responseHandler.NotFound<OrderResponseDto?>(SystemMessages.ORDER_NOT_FOUND);
            }

            var dto = _mapper.Map<OrderResponseDto?>(order);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<OrderResponseDto>> GetOrderByIdAsync(Guid orderId)
        {
            _logger.LogDebug("GetOrderByIdAsync called for OrderId={OrderId}", orderId);

            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found OrderId={OrderId}", orderId);
                return _responseHandler.NotFound<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);
            }

            var dto = _mapper.Map<OrderResponseDto>(order);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<int>> GetTotalOrdersCountAsync(Guid? storeId = null)
        {
            _logger.LogDebug("GetTotalOrdersCountAsync called for StoreId={StoreId}", storeId);
            var count = await _orderRepository.GetTotalOrdersCountAsync(storeId);
            return _responseHandler.Success(count);
        }

        public async Task<Response<decimal>> GetTotalRevenueAsync(Guid? storeId = null)
        {
            _logger.LogDebug("GetTotalRevenueAsync called for StoreId={StoreId}", storeId);
            var revenue = await _orderRepository.GetTotalRevenueAsync(storeId);
            return _responseHandler.Success(revenue);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByStatus(OrderStatus status, Guid? storeId = null)
        {
            _logger.LogDebug("GetOrdersByStatus called for Status={Status}, StoreId={StoreId}", status, storeId);

            if (!Enum.IsDefined(typeof(OrderStatus), status))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_ORDER_STATUS);

            var orders = await _orderRepository.GetOrdersByStatusAsync(status, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            _logger.LogInformation("Retrieved {Count} orders with status {Status}", orders.Count(), status);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersWithDetailsAsync()
        {
            _logger.LogDebug("GetOrdersWithDetailsAsync called");
            var orders = await _orderRepository.GetOrdersWithDetailsAsync();
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            _logger.LogInformation("Retrieved {Count} orders with details", orders.Count());
            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? storeId = null)
        {
            _logger.LogDebug("GetOrdersByDateRangeAsync called Start={Start}, End={End}, StoreId={StoreId}", startDate, endDate, storeId);

            if (startDate > endDate)
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_DATE_RANGE);

            var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            _logger.LogInformation("Retrieved {Count} orders in date range", orders.Count());
            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerAndStatusAsync(string customerId, OrderStatus status)
        {
            _logger.LogDebug("GetOrdersByCustomerAndStatusAsync called for Customer={Customer}, Status={Status}", customerId, status);

            if (string.IsNullOrWhiteSpace(customerId))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.BAD_REQUEST);

            if (!Enum.IsDefined(typeof(OrderStatus), status))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_ORDER_STATUS);

            var orders = await _orderRepository.GetOrdersByCustomerAndStatusAsync(customerId, status);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            _logger.LogInformation("Retrieved {Count} orders for customer {Customer} with status {Status}", orders.Count(), customerId, status);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetTopNOrdersByValueAsync(int n, Guid? storeId = null)
        {
            _logger.LogDebug("GetTopNOrdersByValueAsync called N={N}, StoreId={StoreId}", n, storeId);

            if (n <= 0)
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_INPUT);

            var orders = await _orderRepository.GetTopNOrdersByValueAsync(n, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            _logger.LogInformation("Retrieved top {N} orders by value", n);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetRecentOrdersAsync(int days, Guid? storeId = null)
        {
            _logger.LogDebug("GetRecentOrdersAsync called Days={Days}, StoreId={StoreId}", days, storeId);

            if (days <= 0)
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_INPUT);

            var orders = await _orderRepository.GetRecentOrdersAsync(days, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            _logger.LogInformation("Retrieved {Count} recent orders in last {Days} days", orders.Count(), days);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<Dictionary<OrderStatus, int>>> GetOrderCountByStatusAsync(Guid? storeId = null)
        {
            _logger.LogDebug("GetOrderCountByStatusAsync called StoreId={StoreId}", storeId);
            var counts = await _orderRepository.GetOrderCountByStatusAsync(storeId);
            return _responseHandler.Success(counts);
        }

        public async Task<Response<OrderResponseDto>> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            _logger.LogInformation("UpdateOrderStatusAsync called OrderId={OrderId}, NewStatus={NewStatus}", orderId, newStatus);

            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);

            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.INVALID_ORDER_STATUS);

            var order = await _orderRepository.GetByIdAsync(orderId, true);
            if (order == null)
            {
                _logger.LogWarning("Order not found OrderId={OrderId}", orderId);
                return _responseHandler.NotFound<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);
            }

            // Domain rules: prevent illegal transitions (example)
            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                _logger.LogWarning("Invalid status transition from {Old} to {New} for OrderId={OrderId}", order.Status, newStatus, orderId);
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);
            }

            order.Status = newStatus;
            await _orderRepository.UpdateAsync(order);

            // Post-update actions (release reservations if cancelled)
            if (newStatus == OrderStatus.Cancelled || newStatus == OrderStatus.Expired)
            {
                await ReleaseOrderReservationsAsync(orderId);
            }

            var dto = _mapper.Map<OrderResponseDto>(order);
            _logger.LogInformation("Order status updated for OrderId={OrderId} to {Status}", orderId, newStatus);
            return _responseHandler.Success(dto);
        }
        public async Task<Response<OrderResponseDto?>> CreateOnlineOrderFromCartAsync( string ClientId ,  CreateOnlineOrderRequestDto dto)
        {
            // Validate dto
            if (dto == null)
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.BAD_REQUEST);

            // Validate client
            var client = await _clientRepository.GetByIdAsync(ClientId);
            if (client == null)
            {
                _logger.LogWarning("Client not found ClientId={ClientId}", ClientId);
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.USER_NOT_FOUND);
            }

            // Fetch cart and items
            var cart = await _cartRepository.GetByIdAsync(dto.CartId,true);
            if (cart == null || !string.Equals(cart.ClientId, ClientId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Cart not found or mismatched ClientId={ClientId} CartId={CartId}", ClientId, dto.CartId);
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.CART_NOT_FOUND);
            }

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            if (cartItems == null || !cartItems.Any())
            {
                _logger.LogWarning("Empty cart for CartId={CartId}", cart.Id);
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.CART_EMPTY);
            }

            var order = new OnlineOrder
            {
                Id = Guid.NewGuid(),
                ClientId = ClientId,
                Status = OrderStatus.Pending,
                TotalPrice = cart.TotalPrice,
                OrderType = OrderType.Online,
                ShippingAddressId = dto.deliveryAddressId
            };

            // Begin transaction
            await _orderRepository.BeginTransactionAsync();
            try
            {
                // Process type-specific checks (stock/reservations)
                var processResult = await ProcessOrderByTypeAsync(order, cartItems, null);
                if (!processResult.Success)
                {
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<OrderResponseDto?>(processResult.ErrorMessage);
                }

                // Save order
                var added = await _orderRepository.AddAsync(order);
                if (added == null)
                {
                    _logger.LogError("Failed to add order to repository OrderId={OrderId}", order.Id);
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<OrderResponseDto?>(SystemMessages.SERVER_ERROR);
                }

                // Convert cart items -> order items and persist
                var orderItems = cartItems.Select(ci => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    SubTotal = ci.SubTotal,
                    InvetoryId = ci.InventoryId,
                    ReservationId = ci.ReservationId
                }).ToList();

                var itemsAdded = await _orderRepository.AddOrderItemsAsync(orderItems);
                if (!itemsAdded)
                {
                    _logger.LogError("Failed adding order items for OrderId={OrderId}", order.Id);
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<OrderResponseDto?>(SystemMessages.SERVER_ERROR);
                }

                // Update reservations statuses to ReservedUntilPayment
                var reservationUpdate = await UpdateReservationsForOrderAsync(cartItems);
                if (!reservationUpdate.Success)
                {
                    _logger.LogError("Failed updating reservations for OrderId={OrderId}", order.Id);
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<OrderResponseDto?>(reservationUpdate.ErrorMessage);
                }

                // Commit transaction
                await _orderRepository.CommitTransactionAsync();

                // Schedule expiration job (10 minutes window)
                _backgroundJobService.Schedule(
                    () => HandleExpiredPaymentAsync(order.Id),
                    TimeSpan.FromMinutes(10));

                // Clear/Archive cart
                await _cartRepository.DeleteAsync(cart);
                await _cartRepository.CreateCartAsync(ClientId);

                // Return DTO (with items)
                var respDto = _mapper.Map<OrderResponseDto?>(order);
                respDto.OrderItems = _mapper.Map<IEnumerable<OrderItemResponseDto>>(orderItems);

                _logger.LogInformation("Order created successfully OrderId={OrderId} ClientId={ClientId}", order.Id, ClientId);
                return _responseHandler.Success(respDto, SystemMessages.ORDER_PLACED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CreateOrderFromCartAsync CartId={CartId}", dto.CartId);
                await _orderRepository.RollBackAsync();
                return _responseHandler.Failed<OrderResponseDto?>(SystemMessages.SERVER_ERROR);
            }
        }
        public async Task<Response<PickUpOrderResponseDto?>> CreatePickupOrderFromCartAsync(string ClientId, CreatePickUpOrderRequestDto dto)
        {
            if (dto == null)
                return _responseHandler.BadRequest<PickUpOrderResponseDto?>(SystemMessages.BAD_REQUEST);

            var client = await _clientRepository.GetByIdAsync(ClientId);
            if (client == null)
                return _responseHandler.BadRequest<PickUpOrderResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var cart = await _cartRepository.GetByIdAsync(dto.CartId);
            if (cart == null || !string.Equals(cart.ClientId, ClientId, StringComparison.OrdinalIgnoreCase))
                return _responseHandler.BadRequest<PickUpOrderResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            if (cartItems == null || !cartItems.Any())
                return _responseHandler.BadRequest<PickUpOrderResponseDto?>(SystemMessages.CART_EMPTY);

            Order order = new FromStoreOrder
            {
                Id = Guid.NewGuid(),
                ClientId = ClientId,
                Status = OrderStatus.Pending,
                TotalPrice = cart.TotalPrice,
                OrderType = OrderType.InStore,
                StoreId = dto.storeId
            };

            await _orderRepository.BeginTransactionAsync();

            try
            {
                // Check stock with out-of-stock response
                var processResult = await ProcessOrderByTypeAsync(order, cartItems, dto.storeId);

                if (!processResult.Success)
                {
                    await _orderRepository.RollBackAsync();

                    if (processResult.OutOfStock.Any())
                    {
                        return _responseHandler.BadRequest<PickUpOrderResponseDto?>(
                             new PickUpOrderResponseDto { outOfStocks = processResult.OutOfStock } ,
                            "Some items are out of stock."
                        );
                    }

                    return _responseHandler.Failed<PickUpOrderResponseDto?>(processResult.ErrorMessage);
                }

                var added = await _orderRepository.AddAsync(order);
                if (added == null)
                {
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<PickUpOrderResponseDto?>(SystemMessages.SERVER_ERROR);
                }

                var orderItems = cartItems.Select(ci => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    SubTotal = ci.SubTotal,
                    InvetoryId = ci.InventoryId,
                    ReservationId = ci.ReservationId
                }).ToList();

                if (!await _orderRepository.AddOrderItemsAsync(orderItems))
                {
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<PickUpOrderResponseDto?>(SystemMessages.SERVER_ERROR);
                }

                var reservationUpdate = await UpdateReservationsForOrderAsync(cartItems);
                if (!reservationUpdate.Success)
                {
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<PickUpOrderResponseDto?>(reservationUpdate.ErrorMessage);
                }

                await _orderRepository.CommitTransactionAsync();

                _backgroundJobService.Schedule(
                    () => HandleExpiredPaymentAsync(order.Id),
                    TimeSpan.FromMinutes(10));

                await _cartRepository.DeleteAsync(cart);
                await _cartRepository.CreateCartAsync(ClientId);

                var respDto = _mapper.Map<PickUpOrderResponseDto?>(order);
                //respDto.OrderItems = _mapper.Map<IEnumerable<OrderItemResponseDtoForPickup>>(orderItems);

                return _responseHandler.Success(respDto, SystemMessages.ORDER_PLACED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CreatePickupOrderFromCartAsync CartId={CartId}", dto.CartId);
                await _orderRepository.RollBackAsync();
                return _responseHandler.Failed<PickUpOrderResponseDto?>(SystemMessages.SERVER_ERROR);
            }
        }



        public async Task<Response<bool>> DeleteOrderAsync(Guid orderId)
        {
            _logger.LogInformation("DeleteOrderAsync called for OrderId={OrderId}", orderId);

            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.BAD_REQUEST);

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found for deletion OrderId={OrderId}", orderId);
                return _responseHandler.NotFound<bool>(SystemMessages.ORDER_NOT_FOUND);
            }

            // If pending, release reservations
            if (order.Status == OrderStatus.Pending)
            {
                await ReleaseOrderReservationsAsync(orderId);
            }

            var deleted = await _orderRepository.DeleteAsync(order);
            if (!deleted)
            {
                _logger.LogError("Failed to delete OrderId={OrderId}", orderId);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }

            _logger.LogInformation("Order deleted OrderId={OrderId}", orderId);
            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }

        #endregion

        #region Private helpers

        private bool IsValidStatusTransition(OrderStatus from, OrderStatus to)
        {
            // Example rules — adapt to your domain
            if (from == to) return false;
            if (from == OrderStatus.Cancelled || from == OrderStatus.Completed || from == OrderStatus.Expired) return false;

            // Allow any transition for this template except the disallowed above
            return true;
        }


        private async Task<(bool Success, string ErrorMessage, List<OutOfStockItemDto> OutOfStock)>
            ProcessOrderByTypeAsync(Order order, IEnumerable<CartItem> cartItems, Guid? storeId)
        {
            var outOfStockList = new List<OutOfStockItemDto>();

            if (order is FromStoreOrder storeOrder)
            {
                if (!storeId.HasValue)
                    return (false, SystemMessages.STORE_ID_REQUIRED, outOfStockList);

                var store = await _storeRepository.GetByIdAsync(storeOrder.StoreId);
                if (store == null)
                    return (false, SystemMessages.STORE_NOT_FOUND, outOfStockList);

                foreach (var ci in cartItems)
                {
                    var inventory = await _inventoryRepository.GetStockOfProductInStore(ci.ProductId, storeOrder.StoreId);

                    if (inventory == null || inventory.StockQuantity < ci.Quantity)
                    {
                        outOfStockList.Add(new OutOfStockItemDto
                        {
                            ProductId = ci.ProductId,
                            RequestedQty = ci.Quantity,
                            AvailableQty = inventory?.StockQuantity ?? 0
                        });
                    }
                }

                if (outOfStockList.Any())
                    return (false, "Some items are out of stock.", outOfStockList);

                return (true, string.Empty, outOfStockList);
            }

            if (order is OnlineOrder)
            {
                foreach (var ci in cartItems)
                {
                    if (ci.ReservationId == Guid.Empty) continue;

                    var reservation = await _reservationRepository.GetByIdAsync(ci.ReservationId);
                    if (reservation == null || reservation.Status != ReservationStatus.ReservedUntilCheckout)
                    {
                        return (false, SystemMessages.RESERVATION_INVALID, outOfStockList);
                    }
                }

                return (true, string.Empty, outOfStockList);
            }

            return (false, SystemMessages.INVALID_ORDER_TYPE, outOfStockList);
        }




        private async Task<(bool Success, string ErrorMessage)> UpdateReservationsForOrderAsync(IEnumerable<CartItem> cartItems)
        {
            foreach (var ci in cartItems)
            {
                if (ci.ReservationId == Guid.Empty) continue;

                var reservation = await _reservationRepository.GetByIdAsync(ci.ReservationId);
                if (reservation == null)
                {
                    _logger.LogWarning("Reservation not found for CartItem {CartItemId}", ci.CartItemId);
                    return (false, SystemMessages.RESERVATION_FAILED);
                }

                reservation.Status = ReservationStatus.ReservedUntilPayment;
                var updated = await _reservationRepository.UpdateAsync(reservation);
                if (updated is null)
                {
                    _logger.LogError("Failed to update reservation ReservationId={ReservationId}", reservation.Id);
                    return (false, SystemMessages.RESERVATION_FAILED);
                }
            }

            return (true, string.Empty);
        }

        private async Task ReleaseOrderReservationsAsync(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
                if (order == null) return;

                var items = order.Items ?? Enumerable.Empty<OrderItem>();
                foreach (var item in items)
                {
                    if (item.ReservationId == Guid.Empty) continue;

                    var reservation = await _reservationRepository.GetByIdAsync(item.ReservationId);
                    if (reservation != null)
                    {
                        await _reservationRepository.CancelReservationAsync(reservation.Id,item .InvetoryId ,ReservationStatus.Realesed);
                    }
                }

                _logger.LogInformation("Released reservations for OrderId={OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing reservations for OrderId={OrderId}", orderId);
            }
        }

        public async Task HandleExpiredPaymentAsync(Guid orderId)
        {
            _logger.LogInformation("HandleExpiredPaymentAsync started for OrderId={OrderId}", orderId);
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId, true);
                if (order == null) return;

                if (order.Status != OrderStatus.Pending) return;

                // Release reservations and cancel order
                await ReleaseOrderReservationsAsync(orderId);
                order.Status = OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(order);

                // If payment record exists try to cancel or refund (best-effort)
                if (order.PaymentId != 0)
                {
                    await _paymentService.TryCancelOrRefundAsync(order.Id);
                }

                _logger.LogInformation("Expired payment handled and order cancelled OrderId={OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling expired payment for OrderId={OrderId}", orderId);
            }
        }

        #endregion
    }
}
