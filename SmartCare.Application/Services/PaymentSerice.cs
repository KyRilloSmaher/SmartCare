using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commons;
using SmartCare.Application.DTOs.payment;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Events;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe;
using Stripe.Checkout;

namespace SmartCare.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentGetway _paymentGateway;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IResponseHandler _responseHandler;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentGetway paymentGateway,
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IResponseHandler responseHandler,
            IMapper mapper,
            IEventBus eventBus,
            ILogger<PaymentService> logger)
        {
            _paymentGateway = paymentGateway;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _responseHandler = responseHandler;
            _mapper = mapper;
            _eventBus = eventBus;
            _logger = logger;
        }

        // ✅ Unified Payment Process
        public async Task<Response<Session>> ProcessPaymentAsync(CreateCheckoutSessionRequest req)
        {
            var order = await _orderRepository.GetByIdAsync(req.OrderId, true);
            if (order == null)
                return _responseHandler.BadRequest<Session>(SystemMessages.ORDER_NOT_FOUND);

            var request = new PaymentSessionRequest
            {
                 
                Amount = order.TotalPrice,
                SuccessUrl = $"{req.ReturnUrl}/success/{order.Id}",
                CancelUrl = $"{req.ReturnUrl}/fail/{order.Id}",
                OrderId = order.Id.ToString()
            };

            var session = await _paymentGateway.CreateCheckoutSessionAsync(request);
            var payment = _mapper.Map<Payment>(session);
            payment.OrderId = order.Id;
            payment.Status = PaymentStatus.Pending;
            await _paymentRepository.AddAsync(payment);
            return _responseHandler.Success(session);
        }

        // ✅ Mark Payment as Successful
        public async Task<Response<PaymentResult>> MarkPaymentSuccessAsync(Guid orderId)
        {

            var order = await _orderRepository.GetByIdAsync(orderId, true);
            if (order == null)
                return _responseHandler.BadRequest<PaymentResult>("Order not found.");

            var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
            if (payment == null)
                return _responseHandler.BadRequest<PaymentResult>("Payment not found.");

            order.Status = OrderStatus.Completed;
            payment.Status = PaymentStatus.Completed;

            await _orderRepository.UpdateAsync(order);
            await _paymentRepository.UpdateAsync(payment);

            await _eventBus.PublishAsync(new PaymentStatusChangedEvent(
                order.Id,
                order.ClientId,
                "success",
                "Payment completed successfully!"
            ));
            PaymentResult result = new PaymentResult(true , SystemMessages.PAYMENT_PROCESSED , payment.SessionId);
            return _responseHandler.Success(result);
        }

        // ❌ Mark Payment as Failed
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

            await _orderRepository.UpdateAsync(order);
            await _paymentRepository.UpdateAsync(payment);

            await _eventBus.PublishAsync(new PaymentStatusChangedEvent(
                order.Id,
                order.ClientId,
                "failed",
                "Payment failed or was canceled."
            ));

            PaymentResult result = new PaymentResult(true, SystemMessages.PAYMENT_FAILED, payment.SessionId);
            return _responseHandler.Success(result);
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
    }
}
