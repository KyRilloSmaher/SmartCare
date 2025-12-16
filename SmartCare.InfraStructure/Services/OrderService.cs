using AutoMapper;
using Microsoft.Extensions.Configuration;
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

namespace SmartCare.InfraStructure.Services
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

        private readonly IEventPublisherService _eventPublisherService;
        private readonly ILogger<OrderService> _logger;
        private readonly IConfiguration _configuration;
        private readonly int expirationMinutes;
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
            ILogger<OrderService> logger,
            IConfiguration configuration,
            IEventPublisherService eventPublisherService)
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

            _configuration = configuration;
            expirationMinutes = _configuration.GetValue<int>("ReservationTimes:ForOrderExpirationMinutes");
            _eventPublisherService = eventPublisherService;
        }
        #endregion

        #region Public API (Interface Implementation)

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerIdAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.BAD_REQUEST);

            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.USER_NOT_FOUND);
            }
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(clientId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<OrderResponseDto?>> GetOrderWithDetailsByIdAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.BAD_REQUEST);

            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
            {
                return _responseHandler.NotFound<OrderResponseDto?>(SystemMessages.ORDER_NOT_FOUND);
            }

            var dto = _mapper.Map<OrderResponseDto?>(order);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<OrderResponseDto>> GetOrderByIdAsync(Guid orderId)
        {
           
            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return _responseHandler.NotFound<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);
            }

            var dto = _mapper.Map<OrderResponseDto>(order);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<int>> GetTotalOrdersCountAsync(Guid? storeId = null)
        {
            var count = await _orderRepository.GetTotalOrdersCountAsync(storeId);
            return _responseHandler.Success(count);
        }

        public async Task<Response<decimal>> GetTotalRevenueAsync(Guid? storeId = null)
        {
            var revenue = await _orderRepository.GetTotalRevenueAsync(storeId);
            return _responseHandler.Success(revenue);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByStatus(OrderStatus status, Guid? storeId = null)
        {
          

            if (!Enum.IsDefined(typeof(OrderStatus), status))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_ORDER_STATUS);

            var orders = await _orderRepository.GetOrdersByStatusAsync(status, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersWithDetailsAsync()
        {
           
            var orders = await _orderRepository.GetOrdersWithDetailsAsync();
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? storeId = null)
        {
            if (startDate > endDate)
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_DATE_RANGE);

            var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerAndStatusAsync(string customerId, OrderStatus status)
        {
          
            if (string.IsNullOrWhiteSpace(customerId))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.BAD_REQUEST);

            if (!Enum.IsDefined(typeof(OrderStatus), status))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_ORDER_STATUS);

            var orders = await _orderRepository.GetOrdersByCustomerAndStatusAsync(customerId, status);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetTopNOrdersByValueAsync(int n, Guid? storeId = null)
        {
            if (n <= 0)
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_INPUT);

            var orders = await _orderRepository.GetTopNOrdersByValueAsync(n, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> GetRecentOrdersAsync(int days, Guid? storeId = null)
        {

            if (days <= 0)
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_INPUT);

            var orders = await _orderRepository.GetRecentOrdersAsync(days, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            return _responseHandler.Success(dto);
        }

        public async Task<Response<Dictionary<OrderStatus, int>>> GetOrderCountByStatusAsync(Guid? storeId = null)
        {
           
            var counts = await _orderRepository.GetOrderCountByStatusAsync(storeId);
            return _responseHandler.Success(counts);
        }

        public async Task<Response<OrderResponseDto>> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
           
            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);

            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.INVALID_ORDER_STATUS);

            var order = await _orderRepository.GetByIdAsync(orderId, true);
            if (order == null)
            {
               
                return _responseHandler.NotFound<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);
            }

            // Domain rules: prevent illegal transitions
            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                
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
            
            return _responseHandler.Success(dto);
        }
        public async Task<Response<OrderResponseDto?>> CreateOnlineOrderFromCartAsync(
            string clientId, CreateOnlineOrderRequestDto dto)
        {
            return await CreateOrderFromCartInternalAsync<OrderResponseDto?>(
                clientId,
                dto.CartId,
                OrderType.Online,
                null,
                dto.deliveryAddressId
            );

        }

        public async Task<Response<PickUpOrderResponseDto?>> CreatePickupOrderFromCartAsync(
            string clientId, CreatePickUpOrderRequestDto dto)
        {
            return await CreateOrderFromCartInternalAsync<PickUpOrderResponseDto?>(
                clientId,
                dto.CartId,
                OrderType.InStore,
                dto.storeId,
                null
            );
        }


        public async Task<Response<bool>> DeleteOrderAsync(Guid orderId)
        {
           
            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.BAD_REQUEST);

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
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

        private async Task<Response<T?>> CreateOrderFromCartInternalAsync<T>(string clientId,Guid cartId,OrderType orderType,Guid? storeId,Guid? deliveryAddressId)
        {
            //Validate client
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return _responseHandler.BadRequest<T?>(SystemMessages.USER_NOT_FOUND);

            //Fetch cart
            var cart = await _cartRepository.GetByIdAsync(cartId,true);
            if (cart == null || !string.Equals(cart.ClientId, clientId, StringComparison.OrdinalIgnoreCase))
                return _responseHandler.BadRequest<T?>(SystemMessages.CART_NOT_FOUND);

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            if (!cartItems.Any())
                return _responseHandler.BadRequest<T?>(SystemMessages.CART_EMPTY);

            // Build order object
            Order order = orderType == OrderType.Online
                ? new OnlineOrder
                {
                    Id = Guid.NewGuid(),
                    ClientId = clientId,
                    Status = OrderStatus.Pending,
                    TotalPrice = cart.TotalPrice,
                    OrderType = OrderType.Online,
                    ShippingAddressId = deliveryAddressId.Value
                }
                : new FromStoreOrder
                {
                    Id = Guid.NewGuid(),
                    ClientId = clientId,
                    Status = OrderStatus.Pending,
                    TotalPrice = cart.TotalPrice,
                    OrderType = OrderType.InStore,
                    StoreId = storeId.Value
                };

            //Begin Transaction
            await _orderRepository.BeginTransactionAsync();

            try
            {
                // Check stock & reservations
                var process = await ProcessOrderByTypeAsync(order, cartItems, storeId);
                if (!process.Success)
                {
                    await _orderRepository.RollBackAsync();

                    // special case only for pickup
                    if (typeof(T) == typeof(PickUpOrderResponseDto) && process.OutOfStock.Any())
                    {
                        var pickUpResp = new PickUpOrderResponseDto
                        {
                            outOfStocks = process.OutOfStock
                        };

                        return _responseHandler.BadRequest<T?>(
                                         (T)(object)pickUpResp,
                                        "Some items are out of stock."
                                    );

                    }

                    return _responseHandler.Failed<T?>(process.ErrorMessage);
                }

                //Save order
                var savedOrder = await _orderRepository.AddAsync(order);
                if (savedOrder == null)
                {
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<T?>(SystemMessages.SERVER_ERROR);
                }

                // Save order items
                var orderItems = BuildOrderItems(order.Id, cartItems);
                if (!await _orderRepository.AddOrderItemsAsync(orderItems))
                {
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<T?>(SystemMessages.SERVER_ERROR);
                }

                //Update reservations (ReservedUntilPayment)
                var reservationUpdate = await UpdateReservationsForOrderAsync(cartItems);
                if (!reservationUpdate.Success)
                {
                    await _orderRepository.RollBackAsync();
                    return _responseHandler.Failed<T?>(reservationUpdate.ErrorMessage);
                }

                //Clear cart
                await _cartRepository.DeleteAsync(cart);
                await _cartRepository.CreateCartAsync(clientId);
                //Commit transaction
                await _orderRepository.CommitTransactionAsync();

                //Schedule expiration job
                _backgroundJobService.Schedule(
                    () => ReleaseOrderReservationsAsync(order.Id),
                    TimeSpan.FromMinutes(expirationMinutes));


                //Build Response
                var responseDto = _mapper.Map<T>(order);

                if (responseDto is PickUpOrderResponseDto pickupResp)
                {
                    pickupResp.outOfStocks = process.OutOfStock;
                    foreach (var item in pickupResp.items)
                    {
                        item.IsReadyForPickup = !pickupResp.outOfStocks
                            .Any(o => o.ProductId == item.product.ProductId);
                    }
                }

                return _responseHandler.Success(responseDto,SystemMessages.ORDER_PLACED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error during CreateOrderFromCartInternalAsync CartId={CartId}", cartId);

                await _orderRepository.RollBackAsync();
                return _responseHandler.Failed<T?>(SystemMessages.SERVER_ERROR);
            }
        }
        private List<OrderItem> BuildOrderItems(Guid orderId, IEnumerable<CartItem> cartItems)
        {
            return cartItems.Select(ci => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                SubTotal = ci.SubTotal,
                InvetoryId = ci.InventoryId,
                ReservationId = ci.ReservationId
            }).ToList();
        }
        private async Task<(bool Success, string ErrorMessage, List<OutOfStockItemDto> OutOfStock)> ProcessOrderByTypeAsync(Order order, IEnumerable<CartItem> cartItems, Guid? storeId)
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

                    if (inventory == null || inventory.StockQuantity - inventory.ReservedQuantity < ci.Quantity)
                    {
                        outOfStockList.Add(new OutOfStockItemDto
                        {
                            ProductId = ci.ProductId,
                            RequestedQty = ci.Quantity,
                            AvailableQty = inventory?.StockQuantity ?? 0
                        });
                    }
                    else
                    {
                        var reservation = await _reservationRepository.GetByIdAsync(ci.ReservationId, true);
                        reservation.ExpiredAt = DateTime.UtcNow;
                        await _reservationRepository.UpdateAsync(reservation);
                        await _reservationRepository.CancelReservationAsync(ci.ReservationId, ci.InventoryId, ReservationStatus.Realesed);
                        inventory.ReservedQuantity += ci.Quantity;
                        ci.InventoryId = inventory.Id;
                        await _inventoryRepository.UpdateAsync(inventory);
                        await _cartRepository.UpdateItemCartAsync(ci);
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

                var reservation = await _reservationRepository.GetByIdAsync(ci.ReservationId ,true);
                if (reservation == null)
                {
                    return (false, SystemMessages.RESERVATION_FAILED);
                }

                reservation.Status = ReservationStatus.ReservedUntilPayment;
                reservation.ExpiredAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
                var updated = await _reservationRepository.UpdateAsync(reservation);
                if (updated is null)
                {
                    return (false, SystemMessages.RESERVATION_FAILED);
                }
            }

            return (true, string.Empty);
        }

        public async Task ReleaseOrderReservationsAsync(Guid orderId)
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
                    if (reservation != null && reservation.Status == ReservationStatus.ReservedUntilPayment)
                    {
                        await _reservationRepository.CancelReservationAsync(reservation.Id,item .InvetoryId ,ReservationStatus.OrderTimeOut);

                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        // --- Post-commit jobs
                        if (!product.IsAvailable)
                        {
                            _backgroundJobService.Enqueue<IEventPublisherService>(publisher =>
                                publisher.PublishProductStockStatusChanged(item.ProductId, true));
                        }
                    }
                }
                order.Status= OrderStatus.Expired;
               await _orderRepository.UpdateAsync(order);
                await _eventPublisherService.PublishOrderExpirationNotification(order.ClientId,orderId);
                _logger.LogInformation("Released reservations for OrderId={OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing reservations for OrderId={OrderId}", orderId);
            }
        }


        #endregion
    }
}
