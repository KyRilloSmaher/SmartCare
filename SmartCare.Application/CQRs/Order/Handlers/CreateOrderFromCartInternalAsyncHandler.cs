using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.CQRs.Order.Extension;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class CreateOrderFromCartInternalAsyncHandler<T> : IRequestHandler<CreateOrderFromCartInternalAsyncCommand<T>, Response<T?>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IMapper _mapper;
        private readonly ISqlLockManager _sqlLockManager;
        private readonly ILogger<OrderService> _logger;
        private readonly IEmailService _emailService;
        private readonly int expirationDays;
        private readonly int expirationHours;
        private readonly IMediator _mediator;
        #endregion

        public CreateOrderFromCartInternalAsyncHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IBackgroundJobService backgroundJobService,
            IMapper mapper,
            ISqlLockManager sqlLockManager,
            ILogger<OrderService> logger,
            IEmailService emailService,
            int expirationDays,
            int expirationHours,
            IMediator mediator)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
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
            var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
            if (client == null)
                return _responseHandler.BadRequest<T?>(SystemMessages.USER_NOT_FOUND);

            // 2. Load cart
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartId, true);
            if (cart == null || cart.ClientId != clientId)
                return _responseHandler.BadRequest<T?>(SystemMessages.CART_NOT_FOUND);

            var cartItems = await _unitOfWork.Carts.GetCartItemsAsync(cart.Id);
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
                    var stock = await _unitOfWork.Inventories.GetStockOfProductInStoreAsync(ci.ProductId, storeId!.Value, ci.Quantity);

                    if (stock == null)
                    {
                        step3OutOfStock.Add(OrderExtensions.BuildOutOfStock(ci, 0));
                        continue;
                    }

                    ci.InventoryId = stock.Id;
                }
                else
                {
                    var inventory = await _unitOfWork.Inventories.GetAvailableInventoryAsync(ci.ProductId, ci.Quantity);

                    if (inventory is null)
                    {
                        step3OutOfStock.Add(OrderExtensions.BuildOutOfStock(ci, 0));
                        continue;
                    }

                    ci.InventoryId = inventory.Id;
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

                await _unitOfWork.Orders.AddAsync(order);

                // =====================================================
                // 6. Create order items FIRST
                // =====================================================
                var orderItems = OrderExtensions.BuildOrderItems(order.Id, cartItems);
                await _unitOfWork.Orders.AddOrderItemsAsync(orderItems);

                // =====================================================
                // 7. Create reservations (HARD validation)
                // =====================================================
                var step7OutOfStock = new List<OutOfStockItemDto>();

                foreach (var item in orderItems)
                {
                    var inventory = await _unitOfWork.Inventories.GetByIdAsync(item.InvetoryId);

                    if (inventory.StockQuantity < item.Quantity)
                    {
                        step7OutOfStock.Add(OrderExtensions.BuildOutOfStock(new CartItem { ProductId = item.ProductId, Quantity = inventory.StockQuantity }, inventory.StockQuantity));
                        continue;
                    }

                    var reservationStatus = orderType == OrderType.InStore
                        ? ReservationStatus.ReservedUntilPickup
                        : ReservationStatus.ReservedUntilPayment;

                    var reservation = await _unitOfWork.Reservations.CreateReservationAsync(
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
                await _unitOfWork.Orders.UpdateOrderItemsAsync(orderItems);

                // =====================================================
                // 9. Save all changes atomically through UnitOfWork
                // =====================================================
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // =====================================================
                // 10. Post-commit actions
                // =====================================================
                if (orderType == OrderType.InStore && pickupCode != null)
                {
                    var store = await _unitOfWork.Stores.GetByIdAsync(storeId!.Value);

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