using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Extension;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.Commands.UpdateOrder
{
    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Response<OrderResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IMapper _mapper;
        private readonly ISqlLockManager _sqlLockManager;
        private readonly ILogger<UpdateOrderCommandHandler> _logger;
        private readonly IPaymentGatewayFactory _paymentGatewayFactory;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly int OrderExpirationTimeUntilPayment;
        #endregion


        #region Constructor

        public UpdateOrderCommandHandler(
            IConfiguration configuration,
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IBackgroundJobService backgroundJobService,
            IMapper mapper,
            ISqlLockManager sqlLockManager,
            ILogger<UpdateOrderCommandHandler> logger, 
            IPaymentGatewayFactory paymentGatewayFactory, 
            IEventPublisherService eventPublisherService)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _backgroundJobService = backgroundJobService;
            _mapper = mapper;
            _sqlLockManager = sqlLockManager;
            _paymentGatewayFactory = paymentGatewayFactory;
            OrderExpirationTimeUntilPayment = configuration.GetValue<int>("ReservationTimes:ForOrderExpirationMinutes");
            _eventPublisherService = eventPublisherService;
            _logger = logger;
        }
        #endregion


        public async Task<Response<OrderResponseDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var dto = request.dto;

            var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
            if (client == null)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.USER_NOT_FOUND);

            var cart = await _unitOfWork.Carts.GetByIdAsync(dto.CartId, true);
            if (cart == null || cart.ClientId != clientId)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.CART_NOT_FOUND);

            var cartItems = await _unitOfWork.Carts.GetCartItemsAsync(cart.Id);
            if (!cartItems.Any())
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.CART_EMPTY);

            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(dto.OrderId,true);
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

        private async Task<Response<OrderResponseDto>>  RebuildOrderFromCartAsync(Domain.Entities.Order order, Domain.Entities.Cart cart, IEnumerable<CartItem> cartItems, OrderType newOrderType, Guid? shippingAddressId, Guid? storeId)
        {
            var locks = new List<IAsyncDisposable>();
            var client = await _unitOfWork.Clients.GetByIdAsync(order.ClientId);

            try
            {
                // -----------------------------
                // 1. Resolve inventories
                // -----------------------------
                var outOfStock = new List<OutOfStockItemDto>();
                foreach (var ci in cartItems)
                {
                    var inventory = await _unitOfWork.Inventories.GetByIdAsync(ci.InventoryId);
                    ci.InventoryId = newOrderType == OrderType.InStore ?
                        (
                         await _unitOfWork.Inventories.GetStockOfProductInStoreAsync(ci.ProductId, inventory.StoreId!, ci.Quantity))?.Id ?? Guid.Empty :
                         _unitOfWork.Inventories.GetAvailableInventoryAsync(ci.ProductId, ci.Quantity).Result.Id;

                    if (ci.InventoryId == Guid.Empty)
                        outOfStock.Add(OrderExtensions.BuildOutOfStock(ci, 0));
                }

                if (outOfStock.Any())
                    return BuildStockErrorResponse<OrderResponseDto>(outOfStock);

                // -----------------------------
                // 2. Acquire locks
                // -----------------------------
                foreach (var invId in cartItems.Select(c => c.InventoryId).Distinct().OrderBy(x => x))
                {
                    locks.Add(await _sqlLockManager
                        .AcquireLockAsync($"InventoryRow-{invId}", "Exclusive", 10_000));
                }

                // -----------------------------
                // 3. Cancel old reservations
                // -----------------------------
                foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
                {
                    if (item.ReservationId.HasValue)
                    {
                        await _unitOfWork.Reservations.CancelReservationAsync(
                            item.ReservationId.Value,
                            item.InvetoryId,
                            ReservationStatus.OrderUpdated);
                        await _unitOfWork.Reservations.Delete((Guid)item.ReservationId);
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
                    await _unitOfWork.Orders.SwitchOrderTypeAsync(
                        order,
                        newOrderType,
                        shippingAddressId,
                        storeId);
                }

                // -----------------------------
                // 6. Update base order
                // -----------------------------
                order.TotalPrice = cart.TotalPrice;

                // -----------------------------
                // 7. Create new order items
                // -----------------------------
                var orderItems = OrderExtensions.BuildOrderItems(order.Id, cartItems);
                await _unitOfWork.Orders.AddOrderItemsAsync(orderItems);

                // -----------------------------
                // 8. Create reservations
                // -----------------------------
                var reservationErrors = new List<OutOfStockItemDto>();
                foreach (var item in orderItems)
                {
                    var inventory = await _unitOfWork.Inventories.GetByIdAsync(item.InvetoryId);
                    if (inventory.StockQuantity < item.Quantity)
                    {
                        var cartItem = cartItems.First(c => c.ProductId == item.ProductId);
                        reservationErrors.Add(OrderExtensions.BuildOutOfStock(cartItem, inventory.StockQuantity));
                        continue;
                    }

                    var status = ReservationStatus.ReservedUntilPayment;

                    var reservation = await _unitOfWork.Reservations.CreateReservationAsync(
                        productId: item.ProductId,
                        inventoryId: item.InvetoryId,
                        quantity: item.Quantity,
                        status: status,
                        ExpiredAt : DateTime.UtcNow.AddMinutes(OrderExpirationTimeUntilPayment),
                        OrderItemId: item.Id);

                    item.ReservationId = reservation.Id;
                }

                if (reservationErrors.Any())
                    return BuildStockErrorResponse<OrderResponseDto>(reservationErrors);

                // -----------------------------
                // 9. Payment update
                // -----------------------------
                await HandlePaymnetUpdate(order);


                // -----------------------------
                // 10. Save all changes atomically through UnitOfWork
                // -----------------------------
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                ScheduleOrderExpiration(order);
                ScheduledProductsStatusChanged(orderItems);
                return _responseHandler.Success(
                    _mapper.Map<OrderResponseDto>(order),
                    SystemMessages.ORDER_UPDATED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order rebuild failed OrderId={OrderId}", order.Id);
                return _responseHandler.Failed<OrderResponseDto>(SystemMessages.SERVER_ERROR);
            }
            finally
            {
                foreach (var l in locks)
                    await l.DisposeAsync();
            }
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
        public  async Task RealseOrder(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(orderId);

            if (order is null)
                return;

            // Idempotency: don't re-expire an already finalized order
            if (order.Status is OrderStatus.Expired or OrderStatus.Cancelled or OrderStatus.Completed)
                return;

            if (order.Items is null || !order.Items.Any())
                return;
            var reservationStatus = ReservationStatus.PaymentTimeOut;
            // Realse All Items Reservations
            foreach (var item in order.Items)
            {
                if (!item.ReservationId.HasValue)
                    continue;

               await  _unitOfWork.Reservations.CancelReservationAsync(
                   reservationId: item.ReservationId.Value,
                   inventoryId: item.InvetoryId,
                   status: reservationStatus
               );
            }

            order.Status = OrderStatus.Expired;

            // Save changes through UnitOfWork
           await  _unitOfWork.SaveChangesAsync();
            // Push Notifaction to User
           await  _eventPublisherService.PublishOrderExpirationNotification(order.ClientId, orderId);
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

        public void ScheduleOrderExpiration(Domain.Entities.Order order)
        {
            var delay = TimeSpan.FromMinutes(OrderExpirationTimeUntilPayment);
            _backgroundJobService.Schedule(() => RealseOrder(order.Id), delay);
        }

        public async Task HandlePaymnetUpdate(Domain.Entities.Order order)
        {
            var OldPayment = await _unitOfWork.Payments.GetByIdAsync(order.PaymenId, true);
            if (OldPayment is null)
                return;
            if (OldPayment.Status != PaymentStatus.Pending)
            {
                _logger.LogError($"Trying To Update or Cancel A Payment That Is Not Penidng And Its is {OldPayment.Status} For payment id {OldPayment.Id} and OrderId Is {order.Id} .");
                return;
            }
            // Cancelle Old Payment
            IPaymentGetway paymentService = _paymentGatewayFactory.Resolve(OldPayment.Method);
            await paymentService.CancelSessionAsync(OldPayment.ProviderReferenceId);
            OldPayment.Status = PaymentStatus.Cancelled;
        }
    }
}