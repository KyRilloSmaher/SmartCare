using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Extension;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Order = SmartCare.Domain.Entities.Order;

namespace SmartCare.Application.Features.Orders.Commands.CreatePickUpOrder
{
    public class CreatePickupOrderFromCartAsyncHandler : IRequestHandler<CreatePickupOrderFromCartCommand, Response<PickUpOrderResponseDto?>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IMapper _mapper;
        private readonly ISqlLockManager _sqlLockManager;
        private readonly ILogger<CreatePickupOrderFromCartAsyncHandler> _logger;
        private readonly int OrderExpirationTimeUntilPayment;
        private readonly IEventPublisherService _eventPublisherService;

        public CreatePickupOrderFromCartAsyncHandler(IConfiguration configuration, IResponseHandler responseHandler, IUnitOfWork unitOfWork, IBackgroundJobService backgroundJobService, IMapper mapper, ISqlLockManager sqlLockManager, ILogger<CreatePickupOrderFromCartAsyncHandler> logger, IEventPublisherService eventPublisherService)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _backgroundJobService = backgroundJobService;
            _mapper = mapper;
            _sqlLockManager = sqlLockManager;
            _logger = logger;
            OrderExpirationTimeUntilPayment = configuration.GetValue<int>("ReservationTimes:ForOrderExpirationMinutes");
            _eventPublisherService = eventPublisherService;
        }
        #endregion


        public async Task<Response<PickUpOrderResponseDto?>> Handle(CreatePickupOrderFromCartCommand request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var storeId = request.dto.storeId;
            var cartId = request.dto.CartId;
            // =====================================================
            // 1. Validate client
            // =====================================================
            var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
            if (client == null)
                return _responseHandler.BadRequest<PickUpOrderResponseDto?>(SystemMessages.USER_NOT_FOUND);
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartId, true);
            if (cart == null || cart.ClientId != clientId)
                return _responseHandler.BadRequest<PickUpOrderResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var cartItems = cart.Items;
            if (!cartItems.Any())
                return _responseHandler.BadRequest<PickUpOrderResponseDto?>(SystemMessages.CART_EMPTY);
            // =====================================================
            // 3. Resolve inventories (SOFT validation)
            // =====================================================
            var step3OutOfStock = new List<OutOfStockItemDto>();

            foreach (var ci in cartItems)
            {
                var stock = await _unitOfWork.Inventories.GetStockOfProductInStoreAsync(ci.ProductId, storeId, ci.Quantity);

                if (stock == null)
                {
                    step3OutOfStock.Add(OrderExtensions.BuildOutOfStock(ci, 0));
                    continue;
                }

                ci.InventoryId = stock.Id;
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
            var order = PickUpOrder.Create(clientId, cart.TotalPrice, storeId);
            var pickupCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D7");
            var HasedCode = ComputeSha256(pickupCode);
            order.AddPickUpCode(HasedCode);
            await _unitOfWork.Orders.AddInOfflineOrderAsync(order);
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

                if (inventory is null || inventory.StockQuantity < item.Quantity)
                {
                    step7OutOfStock.Add(OrderExtensions.BuildOutOfStock(new CartItem { ProductId = item.ProductId, Quantity = inventory.StockQuantity }, inventory.StockQuantity));
                    continue;
                }

                var reservationStatus = ReservationStatus.ReservedUntilPayment;

                var reservation = await _unitOfWork.Reservations.CreateReservationAsync(
                    productId: item.ProductId,
                    inventoryId: item.InvetoryId,
                    quantity: item.Quantity,
                    status: reservationStatus,
                    ExpiredAt: DateTime.UtcNow.AddMinutes(OrderExpirationTimeUntilPayment),
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
            var response = _mapper.Map<PickUpOrderResponseDto>(order);
            foreach (var l in inventoryLocks)
                await l.DisposeAsync();
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
                  await  _eventPublisherService.PublishProductStockStatusChanged(id, newStatus);
                }
            }
        }
        private void ScheduleOrderExpiration(Order order)
        {
            var delay = TimeSpan.FromMinutes(OrderExpirationTimeUntilPayment);
            _backgroundJobService.Schedule(() => RealseOrder(order.Id), delay);
        }
        public async Task RealseOrder(Guid orderId)
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

        public static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes); // uppercase hex
        }

        private Response<PickUpOrderResponseDto> BuildStockErrorResponse(List<OutOfStockItemDto> outOfStock)
        {
         return _responseHandler.BadRequest<PickUpOrderResponseDto>(new PickUpOrderResponseDto { outOfStocks = outOfStock },"Some items are out of stock.");
        }
    }
}