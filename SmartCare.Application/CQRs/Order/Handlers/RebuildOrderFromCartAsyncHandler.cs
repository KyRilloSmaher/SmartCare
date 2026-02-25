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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class RebuildOrderFromCartAsyncHandler : IRequestHandler<RebuildOrderFromCartAsyncCommand, Response<OrderResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IMapper _mapper;
        private readonly ISqlLockManager _sqlLockManager;
        private readonly ILogger<OrderService> _logger;
        private readonly IMediator _mediator;
        private readonly int expirationDays;
        private readonly int expirationHours;
        #endregion

        public RebuildOrderFromCartAsyncHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IBackgroundJobService backgroundJobService,
            IMapper mapper,
            ISqlLockManager sqlLockManager,
            ILogger<OrderService> logger,
            IMediator mediator)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _backgroundJobService = backgroundJobService;
            _mapper = mapper;
            _sqlLockManager = sqlLockManager;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Response<OrderResponseDto>> Handle(RebuildOrderFromCartAsyncCommand request, CancellationToken cancellationToken)
        {
            var order = request.order;
            var cartItems = request.cartItems;
            var newOrderType = request.newOrderType;
            var locks = new List<IAsyncDisposable>();
            var storeId = request.storeId;
            var shippingAddressId = request.shippingAddressId;
            var cart = request.cart;

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

                    var status = newOrderType == OrderType.InStore ?
                                     ReservationStatus.ReservedUntilPickup :
                                     ReservationStatus.ReservedUntilPayment;

                    var reservation = await _unitOfWork.Reservations.CreateReservationAsync(
                        productId: item.ProductId,
                        inventoryId: item.InvetoryId,
                        quantity: item.Quantity,
                        status: status,
                        OrderItemId: item.Id);

                    item.ReservationId = reservation.Id;
                }

                if (reservationErrors.Any())
                    return BuildStockErrorResponse<OrderResponseDto>(reservationErrors);

                // -----------------------------
                // 9. Payment update
                // -----------------------------
                await _mediator.Send(new HandlePaymentAsyncCommand(order));

                // -----------------------------
                // 10. In-store pickup logic
                // -----------------------------
                //if (order.OrderType == OrderType.InStore)
                //{
                //    var pickupCode = RandomNumberGenerator
                //        .GetInt32(0, 1_000_000)
                //        .ToString("D7");

                //    await _unitOfWork.Orders.UpdatePickupCodeHashAsync(
                //        order.Id,
                //        ComputeSha256(pickupCode));

                //   await SendPickupEmailAsync(order, client, pickupCode, storeId!.Value);
                //}

                // -----------------------------
                // 11. Save all changes atomically through UnitOfWork
                // -----------------------------
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                ScheduleOrderExpiration(order);

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
    }
}