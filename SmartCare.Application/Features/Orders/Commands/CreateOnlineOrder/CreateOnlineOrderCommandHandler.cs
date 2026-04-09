using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Extension;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.IServices;
using SmartCare.Application.Notifications;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System.Threading;

namespace SmartCare.Application.Features.Orders.Commands.CreateOnlineOrder
{
    public class CreateOnlineOrderCommandHandler : IRequestHandler<CreateOnlineOrderFromCartAsyncCommand, Response<OrderResponseDto?>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IMapper _mapper;
        private readonly ISqlLockManager _sqlLockManager;
        private readonly ILogger<CreateOnlineOrderCommandHandler> _logger;
        private readonly int OrderExpirationTimeUntilPayment;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IOrderNotificationService _notificationService;
        private readonly IMapService _mapService;

        #endregion
        public CreateOnlineOrderCommandHandler(IConfiguration configuration, IResponseHandler responseHandler, IUnitOfWork unitOfWork, IBackgroundJobService backgroundJobService, IMapper mapper, ISqlLockManager sqlLockManager, ILogger<CreateOnlineOrderCommandHandler> logger, IEventPublisherService eventPublisherService, IOrderNotificationService notificationService, IMapService mapService)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _backgroundJobService = backgroundJobService;
            _mapper = mapper;
            _sqlLockManager = sqlLockManager;
            _logger = logger;
            OrderExpirationTimeUntilPayment = configuration.GetValue<int>("ReservationTimes:ForOrderExpirationMinutes");
            _eventPublisherService = eventPublisherService;
            _notificationService = notificationService;
            _mapService = mapService;
        }
        //public CreateOnlineOrderCommandHandler(IConfiguration configuration, IResponseHandler responseHandler, IUnitOfWork unitOfWork, IBackgroundJobService backgroundJobService, IMapper mapper, ISqlLockManager sqlLockManager, ILogger<CreateOnlineOrderCommandHandler> logger, IEventPublisherService eventPublisherService)
        //{
        //    _responseHandler = responseHandler;
        //    _unitOfWork = unitOfWork;
        //    _backgroundJobService = backgroundJobService;
        //    _mapper = mapper;
        //    _sqlLockManager = sqlLockManager;
        //    _logger = logger;
        //    OrderExpirationTimeUntilPayment = configuration.GetValue<int>("ReservationTimes:ForOrderExpirationMinutes");
        //    _eventPublisherService = eventPublisherService;
        //}

        public async  Task<Response<OrderResponseDto?>> Handle(CreateOnlineOrderFromCartAsyncCommand request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var ShippingAddressId = request.dto.deliveryAddressId;
            var cartId = request.dto.CartId;
            // =====================================================
            // 1. Validate client
            // =====================================================
            var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
            if (client == null)
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.USER_NOT_FOUND);
            // =====================================================
            // 2. Load cart
            // =====================================================
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartId, true);
            if (cart == null || cart.ClientId != clientId)
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var cartItems = cart.Items;
            if (!cartItems.Any())
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.CART_EMPTY);
            // =====================================================
            // 3. Resolve inventories (SOFT validation)
            // =====================================================
            var step3OutOfStock = new List<OutOfStockItemDto>();

            foreach (var ci in cartItems)
            {
                    var inventory = await _unitOfWork.Inventories.GetAvailableInventoryAsync(ci.ProductId, ci.Quantity);

                    if (inventory is null)
                    {
                        step3OutOfStock.Add(OrderExtensions.BuildOutOfStock(ci, 0));
                        continue;
                    }

                    ci.InventoryId = inventory.Id;
                
            }

            if (step3OutOfStock.Any())
                return BuildStockErrorResponse(step3OutOfStock);
            // =====================================================
            // 4. Acquire inventory locks
            // =====================================================
            var inventoryLocks = new List<IAsyncDisposable>();
            foreach (var invId in cartItems.Select(c => c.InventoryId).Distinct().OrderBy(x => x))
            {
                inventoryLocks.Add(await _sqlLockManager.AcquireLockAsync($"InventoryRow-{invId}", "Exclusive", 10000));
            }
            // =====================================================
            // 5. Create order
            // =====================================================
            var order = OnlineOrder.Create(clientId, cart.TotalPrice, ShippingAddressId);
            await _unitOfWork.Orders.AddInOnlineOrderAsync(order);
            // =====================================================
            // 6. Create order items
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

                if ( inventory is null || inventory.StockQuantity < item.Quantity)
                {
                    step7OutOfStock.Add(OrderExtensions.BuildOutOfStock(new CartItem { ProductId = item.ProductId, Quantity = inventory.StockQuantity }, inventory.StockQuantity));
                    continue;
                }

                var reservationStatus =ReservationStatus.ReservedUntilPayment;

                var reservation = await _unitOfWork.Reservations.CreateReservationAsync(
                    productId: item.ProductId,
                    inventoryId: item.InvetoryId,
                    quantity: item.Quantity,
                    status: reservationStatus,
                    ExpiredAt:DateTime.UtcNow.AddMinutes(OrderExpirationTimeUntilPayment),
                    OrderItemId: item.Id);
                if (reservation is not null)
                      item.ReservationId = reservation.Id;
            }

            if (step7OutOfStock.Any())
                return BuildStockErrorResponse(step7OutOfStock);
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
            ScheduleOrderExpiration(order);
            ScheduledProductsStatusChanged(orderItems);
            var response = _mapper.Map<OrderResponseDto>(order);
            foreach (var l in inventoryLocks)
                await l.DisposeAsync();

            // =====================================================
            // 11. ✅ SignalR — Notify pharmacists in this branch
            // =====================================================
            try
            {
                var storeId = cartItems.First().Inventory.StoreId;
                var store = await _unitOfWork.Stores.GetByIdAsync(storeId);
                var address = await _unitOfWork.Addresses.GetByIdAsync(ShippingAddressId);

                if (store != null && address != null)
                {
                    var notificationDto = new OnlineOrderResponseDto
                    {
                        OrderId = order.Id,
                        ClientName = $"{client.User.FirstName} {client.User.LastName}",
                        ClientPhone = client.User.PhoneNumber,
                        TotalPrice = order.TotalPrice,
                        Status = order.Status.ToString(),
                        OrderDate = order.CreatedAt,
                        DeliveryAddress = address.AddressLine,
                        AdditionalInfo = address.AdditionalInfo,
                        DistanceFromBranch = Math.Round(_mapService.CalculateDistanceKm(
                            store.Latitude, store.Longitude,
                            address.Latitude, address.Longitude), 2),
                        Items = orderItems.Select(oi => new OnlineOrderItemDto
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.Product?.NameEn,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            SubTotal = oi.SubTotal
                        }).ToList()
                    };

                    await _notificationService.NotifyNewOnlineOrderAsync(storeId, notificationDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SignalR notification failed for order {OrderId}", order.Id);
            }
            return _responseHandler.Success(response, SystemMessages.ORDER_PLACED);

        }
        private void ScheduledProductsStatusChanged(List<OrderItem> orderItems)
        {
            var ProductIdsList = orderItems.Select(Oi => Oi.ProductId).ToList();
            _backgroundJobService.Enqueue(() => UpdateProdcutsStatus(ProductIdsList));
        }
        public async Task UpdateProdcutsStatus(List<Guid> Ids)
        {
            foreach (var id in Ids)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                var oldStatus = product.IsAvailable;
                var newStatus = await _unitOfWork.Products.CalculateProductAvailabilty(id);
                if (newStatus != oldStatus)
                {
                    await _eventPublisherService.PublishProductStockStatusChanged(id, newStatus);
                }
            }
        }
        public void ScheduleOrderExpiration(OnlineOrder order)
        {
            var delay = TimeSpan.FromMinutes(OrderExpirationTimeUntilPayment);
            _backgroundJobService.Schedule(() => RealseOrder(order.Id), delay);
        }
        public async Task  RealseOrder(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(orderId);

            if (order is null)
                  return;

            // Idempotency: don't re-expire an already finalized order
            if (order.Status is OrderStatus.Expired or OrderStatus.Cancelled or OrderStatus.Completed or OrderStatus.Confirmed)
                return ;

            if (order.Items is null || !order.Items.Any())
                return;
            var reservationStatus = ReservationStatus.PaymentTimeOut;
            // Realse All Items Reservations
            foreach (var item in order.Items)
            {
                if (!item.ReservationId.HasValue)
                    continue;

                 await _unitOfWork.Reservations.CancelReservationAsync(
                    reservationId: item.ReservationId.Value,
                    inventoryId: item.InvetoryId,
                    status: reservationStatus
                );
            }

            order.Status = OrderStatus.Expired;

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync();
            // Push Notifaction to User
            await _eventPublisherService.PublishOrderExpirationNotification(order.ClientId, orderId);
        }
        private Response<OrderResponseDto?>  BuildStockErrorResponse(List<OutOfStockItemDto> items)
        {
            return _responseHandler.Success<OrderResponseDto?>(new OrderResponseDto { outOfStocks = items},SystemMessages.ORDER_PLACED);
        }
    }
}
