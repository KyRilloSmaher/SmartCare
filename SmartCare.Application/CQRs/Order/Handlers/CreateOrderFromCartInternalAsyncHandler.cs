using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.CQRs.Order.Extension;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class CreateOrderFromCartInternalAsyncHandler<T> : IRequestHandler<CreateOrderFromCartInternalAsyncCommand<T>, Response<T?>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IReservationRepository _reservationRepository;
        //private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IStoreRepository _storeRepository;
        //private readonly IPaymentService _paymentService;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IMapper _mapper;
        private readonly ISqlLockManager _sqlLockManager;
        //private readonly IEventPublisherService _eventPublisherService;
        private readonly ILogger<OrderService> _logger;
        //private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        //private readonly IPaymentRepository _paymentRepository;
        //private readonly IPaymentGetway _paymentGateway;
        private readonly int expirationDays;
        private readonly int expirationHours;
        private readonly IMediator _mediator;


        #endregion

        public CreateOrderFromCartInternalAsyncHandler(IResponseHandler responseHandler, ICartRepository cartRepository, IClientRepository clientRepository, IOrderRepository orderRepository, IReservationRepository reservationRepository, IInventoryRepository inventoryRepository, IStoreRepository storeRepository, IBackgroundJobService backgroundJobService, IMapper mapper, ISqlLockManager sqlLockManager, ILogger<OrderService> logger, IEmailService emailService, int expirationDays, int expirationHours, IMediator mediator)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _clientRepository = clientRepository;
            _orderRepository = orderRepository;
            _reservationRepository = reservationRepository;
            _inventoryRepository = inventoryRepository;
            _storeRepository = storeRepository;
            _backgroundJobService = backgroundJobService;
            _mapper = mapper;
            _sqlLockManager = sqlLockManager;
            _logger = logger;
            _emailService = emailService;
            this.expirationDays = expirationDays;
            this.expirationHours = expirationHours;
            _mediator = mediator;
        }




        public async Task<Response<T?>> Handle(CreateOrderFromCartInternalAsyncCommand<T> request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var cartId = request.cartId;
            var orderType = request.orderType;
            var storeId = request.storeId;
            var deliveryAddressId = request.deliveryAddressId;
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
                        step3OutOfStock.Add(OrderExtensions.BuildOutOfStock(ci, 0));
                        continue;
                    }

                    ci.InventoryId = stock.Id;
                }
                else
                {
                    var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(ci.ProductId, ci.Quantity);

                    if (inventoryId == Guid.Empty)
                    {
                        step3OutOfStock.Add(OrderExtensions.BuildOutOfStock(ci, 0));
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
                var order = OrderExtensions.BuildOrder(
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

                    storeOrder.PickupCodeHash = OrderExtensions.ComputeSha256(pickupCode);
                }

                await _orderRepository.AddAsync(order);

                // =====================================================
                // 6. Create order items FIRST
                // =====================================================
                var orderItems = OrderExtensions.BuildOrderItems(order.Id, cartItems);
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
                        step7OutOfStock.Add(OrderExtensions.BuildOutOfStock(new CartItem { ProductId = item.ProductId, Quantity = inventory.StockQuantity }, inventory.StockQuantity));
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

                // =====================================================
                // 10. Post-commit actions
                // =====================================================
                if (orderType == OrderType.InStore && pickupCode != null)
                {
                    var store = await _storeRepository.GetByIdAsync(storeId!.Value);

                    var emailBody = SystemMessages.PICKUP_ORDER_EMAIL_TEMPLATE
                        .Replace("{{UserName}}", client.User.UserName)
                        .Replace("{{PickupCode}}", pickupCode)
                        .Replace("{{StoreName}}", store.Name)
                        .Replace("{{StoreAddress}}", store.Address)
                        .Replace("{{OrderDate}}", order.CreatedAt.ToString("MMMM dd, yyyy"))
                        .Replace("{{OrderTotal}}", order.TotalPrice.ToString("C"))
                        .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

                    _backgroundJobService.Schedule(() => _emailService.SendEmailAsync(client.User.Email, "Your Pickup Order Details", emailBody),
                                                    TimeSpan.FromSeconds(5));

                }
                ScheduleOrderExpiration(order);
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
        private void ScheduleOrderExpiration(SmartCare.Domain.Entities.Order order)
        {
            var delay = order.OrderType == OrderType.InStore
                ? TimeSpan.FromDays(expirationDays)
                : TimeSpan.FromHours(expirationHours);

            _backgroundJobService.Schedule(
                () => _mediator.Send(new ReleaseOrderReservationsAsyncCommand(order.Id),
                default),
                delay);
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
    }

}
