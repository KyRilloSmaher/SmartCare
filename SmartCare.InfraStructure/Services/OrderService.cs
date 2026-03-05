using AutoMapper;
using Hangfire;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Index.HPRtree;
using SmartCare.Application.commens;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.Repositories;
using System.Security.Cryptography;
using System.Text;

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
        private readonly ISqlLockManager _sqlLockManager;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly ILogger<OrderService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentGetway _paymentGateway;
        private readonly int expirationDays;
        private readonly int expirationHours;
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
            IEventPublisherService eventPublisherService,
            IEmailService emailService,
            ISqlLockManager sqlLockManager,
            IPaymentRepository paymentRepository,
            IPaymentGetway paymentGateway)
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
            expirationDays = _configuration.GetValue<int>("ReservationTimes:DayForPickUp");
            expirationDays = expirationDays == 0 ? 1 : expirationDays;
            expirationHours = _configuration.GetValue<int>("ReservationTimes:HoursForPayment");
            expirationHours = expirationHours == 0 ? 12 : expirationHours;
            _eventPublisherService = eventPublisherService;
            _emailService = emailService;
            _sqlLockManager = sqlLockManager;
            _paymentRepository = paymentRepository;
            _paymentGateway = paymentGateway;
        }
        #endregion

        #region Read Operations

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

        #endregion

        #region Write Operations
        public async Task<Response<bool>> DeleteOrderAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.BAD_REQUEST);

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return _responseHandler.NotFound<bool>(SystemMessages.ORDER_NOT_FOUND);

            if (order.Status == OrderStatus.Pending)
                await ReleaseOrderReservationsAsync(orderId);

            var deleted = await _orderRepository.DeleteAsync(order);
            if (!deleted)
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);

            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
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
        public async Task<Response<OrderResponseDto>> UpdateOrderAsync(string clientId, UpdateOrderRequestDto dto)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.USER_NOT_FOUND);

            var cart = await _cartRepository.GetByIdAsync(dto.CartId, true);
            if (cart == null || cart.ClientId != clientId)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.CART_NOT_FOUND);

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            if (!cartItems.Any())
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.CART_EMPTY);

            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(dto.OrderId);
            if (order == null || order.ClientId != clientId)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);

            if (order.Status != OrderStatus.Pending)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.ORDER_NOT_EDITABLE);

            return await RebuildOrderFromCartAsync(
                order,
                cart,
                cartItems,
                dto.UpdatedOrderType,
                dto.ShippingAddressId,
                dto.StoreId);
        }

        public async Task<Response<OrderResponseDto?>> CreateOnlineOrderFromCartAsync(string clientId, CreateOnlineOrderRequestDto dto)
        {
            return await CreateOrderFromCartInternalAsync<OrderResponseDto?>(
                clientId, dto.CartId, OrderType.Online, null, dto.deliveryAddressId);
        }
        public async Task<Response<PickUpOrderResponseDto?>> CreatePickupOrderFromCartAsync(string clientId, CreatePickUpOrderRequestDto dto)
        {
            return await CreateOrderFromCartInternalAsync<PickUpOrderResponseDto?>(
                clientId, dto.CartId, OrderType.InStore, dto.storeId, null);
        }

        private async Task<Response<T?>> CreateOrderFromCartInternalAsync<T>(string clientId, Guid cartId, OrderType orderType, Guid? storeId, Guid? deliveryAddressId)
        {
            // 1. Validate client
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return _responseHandler.BadRequest<T?>(SystemMessages.USER_NOT_FOUND);

            // 2. Load cart
            var cart = await _cartRepository.GetByIdAsync(cartId, true);
            if (cart == null || cart.ClientId != clientId)
                return _responseHandler.BadRequest<T?>(SystemMessages.CART_NOT_FOUND);

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            if (!cartItems.Any())
                return _responseHandler.BadRequest<T?>(SystemMessages.CART_EMPTY);

            // =====================================================
            // 3. Resolve inventories (SOFT validation)
            // =====================================================
            var step3OutOfStock = new List<OutOfStockItemDto>();

            foreach (var ci in cartItems)
            {
                if (orderType == OrderType.InStore)
                {
                    var stock = await _inventoryRepository.GetStockOfProductInStore(ci.ProductId, storeId!.Value, ci.Quantity);

                    if (stock == null)
                    {
                        step3OutOfStock.Add(await BuildOutOfStock(ci, 0));
                        continue;
                    }

                    ci.InventoryId = stock.Id;
                }
                else
                {
                    var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(ci.ProductId, ci.Quantity);

                    if (inventoryId == Guid.Empty)
                    {
                        step3OutOfStock.Add(await BuildOutOfStock(ci, 0));
                        continue;
                    }

                    ci.InventoryId = inventoryId;
                }
            }

            if (step3OutOfStock.Any())
                return BuildStockErrorResponse<T>(step3OutOfStock);

            // =====================================================
            // 4. Acquire inventory locks
            // =====================================================
            var inventoryLocks = new List<IAsyncDisposable>();
            foreach (var invId in cartItems.Select(c => c.InventoryId).Distinct().OrderBy(x => x))
            {
                inventoryLocks.Add(
                    await _sqlLockManager.AcquireLockAsync(
                        $"InventoryRow-{invId}", "Exclusive", 10000));
            }

            try
            {
                await _orderRepository.BeginTransactionAsync();

                // =====================================================
                // 5. Create order
                // =====================================================
                var order = BuildOrder(
                    orderType,
                    clientId,
                    cart.TotalPrice,
                    storeId,
                    deliveryAddressId);

                string? pickupCode = null;
                if (order is FromStoreOrder storeOrder)
                {
                    pickupCode = RandomNumberGenerator
                        .GetInt32(0, 1_000_000)
                        .ToString("D7");

                    storeOrder.PickupCodeHash = ComputeSha256(pickupCode);
                }

                await _orderRepository.AddAsync(order);

                // =====================================================
                // 6. Create order items FIRST
                // =====================================================
                var orderItems = BuildOrderItems(order.Id, cartItems);
                await _orderRepository.AddOrderItemsAsync(orderItems);

                // =====================================================
                // 7. Create reservations (HARD validation)
                // =====================================================
                var step7OutOfStock = new List<OutOfStockItemDto>();

                foreach (var item in orderItems)
                {
                    var inventory = await _inventoryRepository.GetByIdAsync(item.InvetoryId);

                    if (inventory.StockQuantity < item.Quantity)
                    {
                        step7OutOfStock.Add(await BuildOutOfStock(new CartItem { ProductId = item.ProductId, Quantity = inventory.StockQuantity }, inventory.StockQuantity));
                        continue;
                    }

                    var reservationStatus = orderType == OrderType.InStore
                        ? ReservationStatus.ReservedUntilPickup
                        : ReservationStatus.ReservedUntilPayment;

                    var reservation = await _reservationRepository.CreateReservationAsync(
                        productId: item.ProductId,
                        inventoryId: item.InvetoryId,
                        quantity: item.Quantity,
                        status: reservationStatus,
                        OrderItemId: item.Id);

                    item.ReservationId = reservation.Id;
                }

                if (step7OutOfStock.Any())
                    return BuildStockErrorResponse<T>(step7OutOfStock);

                // =====================================================
                // 8. Update order items with reservation ids
                // =====================================================
                await _orderRepository.UpdateOrderItemsAsync(orderItems);

                // =====================================================
                // 9. Commit transaction
                // =====================================================
                await _orderRepository.CommitTransactionAsync();
                ScheduleOrderExpiration(order);


                // Signal Each Product Avaliablity 
                foreach (var item in  orderItems)
                {
                    PublishProductStockStatusChanged(item.ProductId, await _inventoryRepository.IsStockAvailableAsync(item.InvetoryId, item.ProductId));
                }
                var response = _mapper.Map<T>(order);
                return _responseHandler.Success(response, SystemMessages.ORDER_PLACED);
            }
            catch (Exception ex)
            {
                await _orderRepository.RollBackAsync();
                _logger.LogError(ex, "CreateOrder failed CartId={CartId}", cartId);
                return _responseHandler.Failed<T?>(SystemMessages.SERVER_ERROR);
            }
            finally
            {
                foreach (var l in inventoryLocks)
                    await l.DisposeAsync();
            }
        }
        private async Task<Response<OrderResponseDto>> RebuildOrderFromCartAsync(Order order,Cart cart,IEnumerable<CartItem> cartItems,OrderType newOrderType,Guid? shippingAddressId,Guid? storeId)
        {
            var locks = new List<IAsyncDisposable>();
            var client = await _clientRepository.GetByIdAsync(order.ClientId);
            try
            {
                // -----------------------------
                // 1. Resolve inventories
                // -----------------------------
                var outOfStock = new List<OutOfStockItemDto>(); 
                foreach (var ci in cartItems)
                {
                    var inventory = await _inventoryRepository.GetByIdAsync(ci.InventoryId); 
                    ci.InventoryId = newOrderType == OrderType.InStore ?
                        (
                         await _inventoryRepository.GetStockOfProductInStore(ci.ProductId, inventory.StoreId!, ci.Quantity))?.Id ?? Guid.Empty :
                         await _inventoryRepository.GetBestInventoryIdAsync(ci.ProductId, ci.Quantity); if (ci.InventoryId == Guid.Empty
                        )
                        outOfStock.Add(await BuildOutOfStock(ci, 0)); 
                }
                if (outOfStock.Any()) return BuildStockErrorResponse<OrderResponseDto>(outOfStock);

                // -----------------------------
                // 2. Acquire locks
                // -----------------------------
                foreach (var invId in cartItems.Select(c => c.InventoryId).Distinct().OrderBy(x => x))
                {
                    locks.Add(await _sqlLockManager
                        .AcquireLockAsync($"InventoryRow-{invId}", "Exclusive", 10_000));
                }

                await _orderRepository.BeginTransactionAsync();

                // -----------------------------
                // 3. Cancel old reservations
                // -----------------------------
                foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
                {
                    if (item.ReservationId.HasValue)
                    {
                        await _reservationRepository.CancelReservationAsync(
                            item.ReservationId.Value,
                            item.InvetoryId,
                            ReservationStatus.OrderUpdated);
                    }
                }

                // -----------------------------
                // 4. Remove old order items
                // -----------------------------
                order.Items?.Clear();

                // -----------------------------
                // 5. Switch order type (SAFE)
                // -----------------------------
                if (order.OrderType != newOrderType)
                {
                    await _orderRepository.SwitchOrderTypeAsync(
                        order,
                        newOrderType,
                        shippingAddressId,
                        storeId);
                }

                // -----------------------------
                // 6. Update base order
                // -----------------------------
                order.TotalPrice = cart.TotalPrice;
               // await _orderRepository.UpdateAsync(order);

                // -----------------------------
                // 7. Create new order items
                // -----------------------------
                var orderItems = BuildOrderItems(order.Id, cartItems);
                await _orderRepository.AddOrderItemsAsync(orderItems);

                // -----------------------------
                // 8. Create reservations
                // -----------------------------

                var reservationErrors = new List<OutOfStockItemDto>();
                foreach (var item in orderItems)
                {
                    var inventory = await _inventoryRepository.GetByIdAsync(item.InvetoryId);
                    if (inventory.StockQuantity < item.Quantity) {
                        var cartItem = cartItems.First(c => c.ProductId == item.ProductId);
                        reservationErrors.Add(await BuildOutOfStock(cartItem, inventory.StockQuantity));
                        continue;
                    } 
                    var status = newOrderType == OrderType.InStore ?
                                     ReservationStatus.ReservedUntilPickup : 
                                     ReservationStatus.ReservedUntilPayment;
                    var reservation = await _reservationRepository.CreateReservationAsync( productId: item.ProductId, inventoryId: item.InvetoryId, quantity: item.Quantity, status: status, OrderItemId: item.Id); 
                    item.ReservationId = reservation.Id;
                }
                if (reservationErrors.Any()) return BuildStockErrorResponse<OrderResponseDto>(reservationErrors);

                // -----------------------------
                // 9. Payment update
                // -----------------------------
                await HandlePaymentAsync(order);

                // -----------------------------
                // 10. In-store pickup logic
                // -----------------------------
                //if (order.OrderType == OrderType.InStore)
                //{
                //    var pickupCode = RandomNumberGenerator
                //        .GetInt32(0, 1_000_000)
                //        .ToString("D7");

                //    await _orderRepository.UpdatePickupCodeHashAsync(
                //        order.Id,
                //        ComputeSha256(pickupCode));

                //   await SendPickupEmailAsync(order, client, pickupCode, storeId!.Value);
                //}

                // -----------------------------
                // 11. Commit
                // -----------------------------
                await _orderRepository.CommitTransactionAsync();

                ScheduleOrderExpiration(order);

                return _responseHandler.Success(
                    _mapper.Map<OrderResponseDto>(order),
                    SystemMessages.ORDER_UPDATED);
            }
            catch (Exception ex)
            {
                await _orderRepository.RollBackAsync();
                _logger.LogError(ex, "Order rebuild failed OrderId={OrderId}", order.Id);
                return _responseHandler.Failed<OrderResponseDto>(SystemMessages.SERVER_ERROR);
            }
            finally
            {
                foreach (var l in locks)
                    await l.DisposeAsync();
            }
        }



        // =====================================================
        // HELPERS
        // =====================================================
        private Order BuildOrder(OrderType orderType, string clientId, decimal totalPrice, Guid? storeId, Guid? deliveryAddressId)
        {
            return orderType switch
            {
                OrderType.InStore => new FromStoreOrder
                {
                    ClientId = clientId,
                    TotalPrice = totalPrice,
                    StoreId = storeId!.Value,
                    OrderType = OrderType.InStore
                },

                OrderType.Online => new OnlineOrder
                {
                    ClientId = clientId,
                    TotalPrice = totalPrice,
                    ShippingAddressId = deliveryAddressId!.Value,
                    OrderType = OrderType.Online
                },

                _ => throw new ArgumentOutOfRangeException(nameof(orderType))
            };
        }

        private void ScheduleOrderExpiration(Order order)
        {
            var delay = order.OrderType == OrderType.InStore
                ? TimeSpan.FromDays(expirationDays)
                : TimeSpan.FromHours(expirationHours);

            _backgroundJobService.Schedule(
                () => ReleaseOrderReservationsAsync(order.Id),
                delay);
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
                ReservationId = null // set later after reservation creation
            }).ToList();
        }


        private async Task<OutOfStockItemDto> BuildOutOfStock(CartItem ci, int available)
        {
            var product = await _productRepository.GetByIdAsync(ci.ProductId);
            return new OutOfStockItemDto
            {

                ProductId = ci.ProductId,
                ProductName = product.NameEn,
                RequestedQty = ci.Quantity,
                AvailableQty = available
            };
        }



        private async Task HandlePaymentAsync(Order order)
        {
            var payment = order.Payment;

            // No payment yet → create new
            if (payment == null)
            {
            //    var newPayment = new Payment
            //    {
            //        OrderId = order.Id,
            //        Amount = order.TotalPrice,
            //        Status = PaymentStatus.Pending,
            //        Version = 1,
            //        Method = PaymentMethod.Cash,
            //        CreatedAt = DateTime.UtcNow
            //    };

            //    await _paymentRepository.AddAsync(newPayment);

            //    order.PaymentIntentId = null;
            //    order.PaymentVersion = newPayment.Version;
               return;
            }

            // Pending → update amount + intent
            if (payment.Status == PaymentStatus.Pending)
            {
                payment.Amount = order.TotalPrice;
                payment.Version += 1;
                payment.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(payment.PaymentIntentId))
                {
                    await _paymentGateway
                        .UpdatePaymentIntentAmountAsync(
                            payment.PaymentIntentId,
                            order.TotalPrice);

                    order.PaymentIntentId = payment.PaymentIntentId;
                    order.PaymentVersion = payment.Version;
                }

                await _paymentRepository.UpdateAsync(payment);
                return;
            }

            // Paid / Failed → create new payment version
            var replacement = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalPrice,
                Status = PaymentStatus.Pending,
                Version = payment.Version + 1,
                Method = PaymentMethod.Cash,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(replacement);

            order.PaymentIntentId = null;
            order.PaymentVersion = replacement.Version;
        }

        private Response<T?> BuildStockErrorResponse<T>(List<OutOfStockItemDto> outOfStock)
        {
            if (typeof(T) == typeof(PickUpOrderResponseDto))
            {
                return _responseHandler.BadRequest<T?>(
                    (T)(object)new PickUpOrderResponseDto { outOfStocks = outOfStock },
                    "Some items are out of stock.");
            }

            return _responseHandler.Failed<T?>(SystemMessages.INSUFFICIENT_STOCK);
        }

        // =====================================================
        // RESERVATION RELEASE
        // =====================================================
        public async Task ReleaseOrderReservationsAsync(Guid orderId)
        {
            _logger.LogInformation("Releasing reservations for OrderId={OrderId}", orderId);
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
                return;

            // Idempotency: don't re-expire an already finalized order
            if (order.Status is OrderStatus.Expired or OrderStatus.Cancelled or OrderStatus.Completed)
                 return;

            if (order.Items == null || !order.Items.Any())
                return;

            var reservationStatus = order.OrderType == OrderType.Online
                ? ReservationStatus.PaymentTimeOut
                : ReservationStatus.PickUpExpired;

            foreach (var item in order.Items)
            {
                if (!item.ReservationId.HasValue)
                    continue;

                await _reservationRepository.CancelReservationAsync(
                    reservationId: item.ReservationId.Value,
                    inventoryId: item.InvetoryId,
                    status: reservationStatus
                );
                PublishProductStockStatusChanged(item.ProductId, await _inventoryRepository.IsStockAvailableAsync(item.InvetoryId, item.ProductId));
            }

            order.Status = OrderStatus.Expired;
            await _orderRepository.UpdateAsync(order);
        }
        private bool IsValidStatusTransition(OrderStatus from, OrderStatus to)
        {
            // Example rules — adapt to your domain
            if (from == to) return false;
            if (from == OrderStatus.Cancelled || from == OrderStatus.Completed || from == OrderStatus.Expired) return false;

            // Allow any transition for this template except the disallowed above
            return true;
        }
        private string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes); // uppercase hex
        }
        private void PublishProductStockStatusChanged(Guid productId, bool isAvailable)
        {
            _backgroundJobService.Enqueue<IEventPublisherService>(publisher =>
                publisher.PublishProductStockStatusChanged(productId, isAvailable));
        }
        #endregion
    }
}

