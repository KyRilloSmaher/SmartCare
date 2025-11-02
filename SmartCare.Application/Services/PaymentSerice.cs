using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using SmartCare.Application.commons;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Events;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe.Checkout;

namespace SmartCare.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentGetway _stripeService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IEventBus _eventBus;

        public PaymentService(
            IPaymentGetway stripeService,
            IPaymentRepository paymentRepository,
            IMapper mapper,
            IOrderRepository orderRepository,
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService,
           IEventBus eventBus)
        {
            _stripeService = stripeService;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _orderRepository = orderRepository;
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _eventBus = eventBus;
        }

        // ✅ Create Checkout Session
        public async Task<Response<Session>> HandleAsync(CreateCheckoutSessionRequest req)
        {
            var order = await _orderRepository.GetByIdAsync(req.OrderId, true);
            if (order == null)
                return _responseHandler.BadRequest<Session>(SystemMessages.ORDER_NOT_FOUND);

            order.Status = OrderStatus.Pending;
            await _orderRepository.UpdateAsync(order);

            var successUrl = $"{req.ReturnUrl}?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{req.ReturnUrl}/cancel";

            var session = await _stripeService.CreateCheckoutSessionAsync(
                order.TotalPrice,
                successUrl,
                cancelUrl,
                order.Id.ToString(),
                "egp",
                new Dictionary<string, string>
                {
                    ["orderId"] = order.Id.ToString(),
                    ["userId"] = order.ClientId
                });

            var payment = _mapper.Map<Payment>(session);
            payment.OrderId = order.Id;
            payment.Status = PaymentStatus.Pending;
            await _paymentRepository.AddAsync(payment);

            return _responseHandler.Success(session);
        }

        // ✅ Called when payment succeeds
        public async Task<Response<string>> MarkPaymentSuccessAsync(string orderId)
        {
            var order = await _orderRepository.GetByIdAsync(Guid.Parse(orderId), true);
            if (order == null)
                return _responseHandler.BadRequest<string>("Order not found.");

            var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
            if (payment == null)
                return _responseHandler.BadRequest<string>("Payment not found.");

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

            return _responseHandler.Success("Payment marked as successful.");
        }

        // ❌ Called when payment fails
        public async Task<Response<string>> MarkPaymentFailureAsync(string orderId)
        {
            var order = await _orderRepository.GetByIdAsync(Guid.Parse(orderId), true);
            if (order == null)
                return _responseHandler.BadRequest<string>("Order not found.");

            var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
            if (payment == null)
                return _responseHandler.BadRequest<string>("Payment not found.");

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

            return _responseHandler.Success("Payment marked as failed.");
        }
    }
}
