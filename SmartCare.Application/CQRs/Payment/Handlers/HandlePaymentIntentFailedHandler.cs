using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Commands;
using SmartCare.Application.CQRs.Payment.Extensions;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class HandlePaymentIntentFailedHandler : IRequestHandler<HandlePaymentIntentFailedAsyncCommand, Unit>
    {

        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IBackgroundJobService _backgroundJobs;
        private readonly PaymentExtensions _paymentExtensions;

        public HandlePaymentIntentFailedHandler(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IBackgroundJobService backgroundJobs, PaymentExtensions paymentExtensions)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _backgroundJobs = backgroundJobs;
            _paymentExtensions = paymentExtensions;
        }

        public async Task<Unit> Handle(HandlePaymentIntentFailedAsyncCommand request, CancellationToken cancellationToken)
        {
            var stripeEvent = request.stripeEvent;
            if (stripeEvent.Data.Object is not PaymentIntent intent)
                return Unit.Value;

            if (!intent.Metadata.TryGetValue("orderId", out var orderIdStr) ||
                !Guid.TryParse(orderIdStr, out var orderId))
                return Unit.Value;

            var order = await _orderRepository.GetByIdAsync(orderId, true);
            if (order == null) return Unit.Value;

            if (order.Status != OrderStatus.Pending) return Unit.Value;

            order.Status = OrderStatus.PaymentFailed;

            var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
            if (payment == null) return Unit.Value;

            payment.Status = PaymentStatus.Completed;


            await _orderRepository.UpdateAsync(order);
            await _paymentRepository.UpdateAsync(payment);

            _paymentExtensions.PublishPaymentEvent(order, "failed", "Payment failed");
            return Unit.Value;
        }
    }
}
