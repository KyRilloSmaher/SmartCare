using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.DTOs.payment;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Messaging;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe;
using Stripe.Checkout;

namespace SmartCare.InfraStructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentGetway _paymentGateway;
        private readonly IClientRepository _clientRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IResponseHandler _responseHandler;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PaymentService> _logger;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IConfiguration _configuration;
        private readonly int PaymentExpirationMinutes;

        public PaymentService(
            IPaymentGetway paymentGateway,
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IResponseHandler responseHandler,
            IMapper mapper,
            IEventBus eventBus,
            ILogger<PaymentService> logger,
            IInventoryRepository inventoryRepository,
            IClientRepository clientRepository,
            IReservationRepository reservationRepository,
            IBackgroundJobService backgroundJobService,
            IConfiguration configuration)
        {
            _paymentGateway = paymentGateway;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository; 
            _responseHandler = responseHandler;
            _mapper = mapper;
            _eventBus = eventBus;
            _logger = logger;
            _inventoryRepository = inventoryRepository;
            _clientRepository = clientRepository;
            _reservationRepository = reservationRepository;
            _backgroundJobService = backgroundJobService;
            _configuration = configuration;
            PaymentExpirationMinutes = _configuration.GetValue<int?>("ReservationTimes:ForPaymentExpirationMinutes") ?? 10;
        }


        public async Task<Response<SessionResponse>> ProcessPaymentAsync(CreateCheckoutSessionRequest req)
        {
            var order = await _orderRepository.GetByIdAsync(req.OrderId, true);
            if (order == null)
                return _responseHandler.BadRequest<SessionResponse>(SystemMessages.ORDER_NOT_FOUND);

            var request = new PaymentSessionRequest
            {
                Amount = order.TotalPrice,
                SuccessUrl = $"{req.ReturnUrl}/success/{order.Id}",
                CancelUrl = $"{req.ReturnUrl}/fail/{order.Id}",
                OrderId = order.Id.ToString()
            };

            var session = await _paymentGateway.CreateCheckoutSessionAsync(request);
            var payment = _mapper.Map<Payment>(session);
            payment.Status = PaymentStatus.Pending;
            payment.OrderId = order.Id;

            // persist payment
            var addedPayment = await _paymentRepository.AddAsync(payment);
            if (addedPayment == null)
            {
                _logger.LogError("Failed to create payment for order {OrderId}", order.Id);
                return _responseHandler.Failed<SessionResponse>(SystemMessages.SERVER_ERROR);
            }

            // persist order.PaymentId
            order.PaymentId = addedPayment.Id;
            var updateOrderResult = await _orderRepository.UpdateAsync(order);
            if (updateOrderResult is null)
            {
                _logger.LogError("Failed to attach payment id to order {OrderId}", order.Id);
                return _responseHandler.Failed<SessionResponse>(SystemMessages.SERVER_ERROR);
            }

            var UpdateResult = await UpdateReservationsForOrderAsync(order.Id);
            if (!UpdateResult.Success)
            {
                // best effort: try to remove payment if reservation update failed
                _logger.LogWarning("Reservation update failed for Order {OrderId}: {Error}", order.Id, UpdateResult.ErrorMessage);
                return _responseHandler.BadRequest<SessionResponse>(UpdateResult.ErrorMessage);
            }

            var response = new SessionResponse
            {
                url = session.Url,
                Id = session.Id
            };

            // schedule expiration job (uses closure — keep as-is to respect structure)
            _backgroundJobService.Schedule(
                () => HandleExpiredPaymentAsync(order.Id),
                TimeSpan.FromMinutes(PaymentExpirationMinutes));

            return _responseHandler.Success(response);
        }

        public async Task<Response<PaymentResult>> MarkPaymentSuccessAsync(Guid orderId)
        {

            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
                return _responseHandler.BadRequest<PaymentResult>("Order not found.");

            var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
            if (payment == null)
                return _responseHandler.BadRequest<PaymentResult>("Payment not found.");

            // Change domain state
            order.Status = OrderStatus.Confirmed;
            order.PaymentId = payment.Id;
            payment.Status = PaymentStatus.Completed;

            // finalize stocks & complete reservations
            var orderItems = order.Items ?? Enumerable.Empty<OrderItem>();
            foreach (var item in orderItems)
            {
                // Best-effort: wrap each item in try/catch so one failure won't prevent others,
                // but still bubble up if something critical fails.
                try
                {
                    await _inventoryRepository.FinalizeStockDeductionAsync(item.InvetoryId, item.Quantity);
                    await _reservationRepository.CancelReservationAsync(item.ReservationId, item.InvetoryId, ReservationStatus.Completed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error finalizing inventory/reservation for Order {OrderId} Item {InventoryId}", order.Id, item.InvetoryId);
                    // continue to next item — we still want to persist overall success where possible
                }
            }

            // update client orders count
            var client = await _clientRepository.GetByIdAsync(order.ClientId, true);
            if (client != null)
            {
                client.OrdersCount += 1;
                try
                {
                    await _clientRepository.UpdateAsync(client);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update client OrdersCount for client {ClientId}", order.ClientId);
                }
            }
            else
            {
                _logger.LogWarning("Client {ClientId} not found when marking payment success.", order.ClientId);
            }

            // persist order and payment
            await _orderRepository.UpdateAsync(order);
            await _paymentRepository.UpdateAsync(payment);

            // publish event
            _backgroundJobService.Enqueue<IEventPublisherService>(publisher =>
                publisher.PublishPaymentStatusChanged(
                    order.Id,
                    order.ClientId,
                    "success",
                    "Payment completed successfully!"
                )
            );

            var result = new PaymentResult(true, SystemMessages.PAYMENT_PROCESSED, payment.SessionId);
            return _responseHandler.Success(result);
        }

        public async Task<Response<PaymentResult>> MarkPaymentFailureAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, true);
            if (order == null)
                return _responseHandler.BadRequest<PaymentResult>("Order not found.");

            var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
            if (payment == null)
                return _responseHandler.BadRequest<PaymentResult>("Payment not found.");

            order.Status = OrderStatus.PaymentFailed;
            payment.Status = PaymentStatus.Failed;

            foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                try
                {
                    await _reservationRepository.CancelReservationAsync(item.ReservationId, item.InvetoryId, ReservationStatus.PaymentFailed);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cancel reservation {ReservationId} while marking payment failure for order {OrderId}", item.ReservationId, order.Id);
                }
            }

            await _orderRepository.UpdateAsync(order);
            await _paymentRepository.UpdateAsync(payment);

            _backgroundJobService.Enqueue<IEventPublisherService>(publisher =>
                publisher.PublishPaymentStatusChanged(
                    order.Id,
                    order.ClientId,
                    "failed",
                    "Payment failed or was canceled."
                )
            );

            // Return failed result
            return _responseHandler.Failed<PaymentResult>(SystemMessages.PAYMENT_FAILED);
        }


        public async Task HandleWebhookEventAsync(Event webhookEvent)
        {
            switch (webhookEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted(webhookEvent);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentFailed(webhookEvent);
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", webhookEvent.Type);
                    break;
            }
        }

        private async Task HandleCheckoutSessionCompleted(Event webhookEvent)
        {
            var session = webhookEvent.Data.Object as Session;
            if (session == null) return;

            if (session.Metadata.TryGetValue("orderId", out var orderIdString) &&
                Guid.TryParse(orderIdString, out var orderId))
            {
                await MarkPaymentSuccessAsync(orderId);
                _logger.LogInformation("✅ Payment successful for order {OrderId}", orderId);
            }
            else
            {
                _logger.LogWarning("⚠️ Missing or invalid orderId in metadata");
            }
        }

        private async Task HandlePaymentFailed(Event webhookEvent)
        {
            var paymentIntent = webhookEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            if (paymentIntent.Metadata.TryGetValue("orderId", out var orderIdString) &&
                Guid.TryParse(orderIdString, out var orderId))
            {
                await MarkPaymentFailureAsync(orderId);
                _logger.LogInformation("❌ Payment failed for order {OrderId}", orderId);
            }
            else
            {
                _logger.LogWarning("⚠️ Missing or invalid orderId in metadata for failed payment");
            }
        }
        public async Task<Response<PaymentResult>> TryCancelOrRefundAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, true);
            if (order == null)
                return _responseHandler.BadRequest<PaymentResult>(SystemMessages.ORDER_NOT_FOUND);

            var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
            if (payment == null)
                return _responseHandler.BadRequest<PaymentResult>(SystemMessages.NOT_FOUND);

            // Case 1: Pending -> cancel
            if (payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Failed;
                order.Status = OrderStatus.Cancelled;

                await _paymentRepository.UpdateAsync(payment);
                await _orderRepository.UpdateAsync(order);

                _backgroundJobService.Enqueue<IEventPublisherService>(publisher =>
                    publisher.PublishPaymentStatusChanged(
                        order.Id,
                        order.ClientId,
                        "cancelled",
                        "Order and payment have been cancelled."
                    )
                );

                return _responseHandler.Success(new PaymentResult(true, "Payment cancelled successfully", payment.SessionId));
            }

            // Case 2: Completed -> attempt refund
            if (payment.Status == PaymentStatus.Completed)
            {
                try
                {
                    var refundSuccess = await _paymentGateway.RefundPaymentAsync(payment.SessionId);

                    if (!refundSuccess)
                        return _responseHandler.BadRequest<PaymentResult>("Refund failed at payment gateway.");

                    payment.Status = PaymentStatus.Refunded;
                    order.Status = OrderStatus.Refunded;

                    await _paymentRepository.UpdateAsync(payment);
                    await _orderRepository.UpdateAsync(order);

                    _backgroundJobService.Enqueue<IEventPublisherService>(publisher =>
                        publisher.PublishPaymentStatusChanged(
                            order.Id,
                            order.ClientId,
                            "refunded",
                            "Payment refunded successfully."
                        )
                    );

                    return _responseHandler.Success(new PaymentResult(true, "Payment refunded successfully", payment.SessionId));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing refund for OrderId: {OrderId}", orderId);
                    return _responseHandler.Failed<PaymentResult>("An error occurred while processing refund.");
                }
            }

            // Case 3: Already refunded or failed
            return _responseHandler.BadRequest<PaymentResult>(
                $"Cannot cancel or refund payment in '{payment.Status}' status."
            );
        }
        public async Task HandleExpiredPaymentAsync(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId, true);
                if (order == null) return;

                if (order.Status != OrderStatus.Pending) return;

                // Release reservations and cancel order
                await ReleaseOrderReservationsAsync(orderId);
                order.Status = OrderStatus.Expired;
                await _orderRepository.UpdateAsync(order);

                // If payment record exists try to cancel or refund (best-effort)
                if (order.PaymentId != 0)
                {
                    await TryCancelOrRefundAsync(order.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during HandleExpiredPaymentAsync for OrderId {OrderId}", orderId);
            }
        }

        private async Task<(bool Success, string ErrorMessage)> UpdateReservationsForOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("UpdateReservationsForOrderAsync: order {OrderId} not found", orderId);
                return (false, SystemMessages.RESERVATION_FAILED);
            }

            foreach (var oi in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                if (oi.ReservationId == Guid.Empty) continue;

                var reservation = await _reservationRepository.GetByIdAsync(oi.ReservationId, true);
                if (reservation == null)
                {
                    return (false, SystemMessages.RESERVATION_FAILED);
                }

                reservation.Status = ReservationStatus.Extra;
                reservation.ExpiredAt = DateTime.UtcNow.AddMinutes(PaymentExpirationMinutes);
                var updated = await _reservationRepository.UpdateAsync(reservation);
                if (updated is null)
                {
                    return (false, SystemMessages.RESERVATION_FAILED);
                }
            }

            return (true, string.Empty);
        }

        private async Task ReleaseOrderReservationsAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("ReleaseOrderReservationsAsync: order {OrderId} not found", orderId);
                return;
            }

            foreach (var oi in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                if (oi.ReservationId == Guid.Empty) continue;

                try
                {
                    await _reservationRepository.CancelReservationAsync(oi.ReservationId, oi.InvetoryId, ReservationStatus.PaymentTimeOut);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cancel reservation {ReservationId} during expiration for order {OrderId}", oi.ReservationId, orderId);
                }
            }
        }
    }
}
