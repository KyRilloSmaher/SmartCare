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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class HandlePaymentIntentFailedHandler : IRequestHandler<HandlePaymentIntentFailedAsyncCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobService _backgroundJobs;
        private readonly PaymentExtensions _paymentExtensions;

        public HandlePaymentIntentFailedHandler(
            IUnitOfWork unitOfWork,
            IBackgroundJobService backgroundJobs,
            PaymentExtensions paymentExtensions)
        {
            _unitOfWork = unitOfWork;
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

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId, true);
            if (order == null) return Unit.Value;

            if (order.Status != OrderStatus.Pending) return Unit.Value;

            order.Status = OrderStatus.PaymentFailed;

            var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
            if (payment == null) return Unit.Value;

            payment.Status = PaymentStatus.Completed;


            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _paymentExtensions.PublishPaymentEvent(order, "failed", "Payment failed");
            return Unit.Value;
        }
    }
}