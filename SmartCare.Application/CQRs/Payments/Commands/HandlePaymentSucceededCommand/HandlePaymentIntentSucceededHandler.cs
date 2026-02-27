
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Extensions;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System.Security.Cryptography;

namespace SmartCare.Application.CQRs.Payments.Commands.HandlePaymentSucceededCommand
{
    public class HandlePaymentIntentSucceededHandler : IRequestHandler<HandlePaymentSucceededAsyncCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandlePaymentIntentSucceededHandler> _logger;
        private readonly PaymentExtensions _paymentExtensions;
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;

        public HandlePaymentIntentSucceededHandler(
            IUnitOfWork unitOfWork,
            ILogger<HandlePaymentIntentSucceededHandler> logger,
            PaymentExtensions paymentExtensions,
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paymentExtensions = paymentExtensions;
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
        }


        public async Task<Response<bool>> Handle(HandlePaymentSucceededAsyncCommand request, CancellationToken cancellationToken)
        {
            Guid OrderId = (Guid)request.paymentwebHookEventResult.OrderId;
            var paymentResult = request.paymentwebHookEventResult;
            // Load order
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(OrderId);

            if (order is null)
            {
                _logger.LogError("Handle payment Successed Method : Trying To Fetch Order With Null Id");
                return _responseHandler.Failed<bool>("Failed To handle payment Success");
            }
            // Validate Payment Values
            var existingPayment = await _unitOfWork.Payments.GetPendingPaymentByOrderIdAsync(OrderId,true);
            if (existingPayment is null || existingPayment.ProviderReferenceId != paymentResult.ProviderReferenceId)
            {
                _logger.LogError("Handle payment Successed Method : No Existing payment For this order");
                return _responseHandler.Failed<bool>();
            }
            var paidAmount = paymentResult.Amount / 100m;
            if (decimal.Round(order.TotalPrice, 2) != decimal.Round((decimal)paidAmount, 2))
            {
                _logger.LogError("Handle payment Successed Method : Miss Matching In Payment Amount");
                return _responseHandler.Failed<bool>();
            }
            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogError("Handle payment Successed Method : Order Status Is not Pending");
                return _responseHandler.Failed<bool>();
            }
            // Finailze Stock Reservation
            foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                try
                {
                    await _unitOfWork.Inventories.FinalizeStockDeductionAsync(
                        item.InvetoryId,
                        item.Quantity,
                        order is FromStoreOrder
                    );
                    await _unitOfWork.Reservations.UpdateReservationStatusAsync(
                        (Guid)item.ReservationId,
                        ReservationStatus.Completed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Inventory finalization failed. Order {OrderId}, Inventory {InventoryId}",
                        order.Id, item.InvetoryId);
                }
            }
            // Confirm order
            order.Status = OrderStatus.Confirmed;
            existingPayment.MarkCompleted();
            // Clear cart
            var cart = await _unitOfWork.Carts.GetActiveCartAsync(order.ClientId);
            if (cart != null)
            {
                await _unitOfWork.Carts.DeleteAsync(cart);
                await _unitOfWork.Carts.CreateCartAsync(order.ClientId);
            }
            // Increment Client orders
            await _paymentExtensions.IncrementClientOrdersAsync(order.ClientId);

            // Save all changes atomically
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _paymentExtensions.PublishPaymentEvent(order, "success", "Payment completed successfully.");

            _backgroundJobService.Enqueue(() => sendEmail(order));
            return _responseHandler.Success(true ,SystemMessages.PAYMENT_PROCESSED);
        }

        void sendEmail(Domain.Entities.Order order)
        {
            var client = _unitOfWork.UserManager.FindByIdAsync(order.ClientId);
            if (order.OrderType == OrderType.Online)
            {
                _paymentExtensions.SendOrderConfirmationEmailAsync(order, client.Result);
            }
            else
            {
                var pickupCode = RandomNumberGenerator
                                    .GetInt32(0, 1_000_000)
                                    .ToString("D7");

                 _unitOfWork.Orders.UpdatePickupCodeHashAsync(order.Id,_paymentExtensions.ComputeSha256(pickupCode));
                 _paymentExtensions.SendPickupEmailAsync(order, client.Result, pickupCode, ((FromStoreOrder)order).StoreId);

            }
            _unitOfWork.SaveChangesAsync();
        }
    }
}