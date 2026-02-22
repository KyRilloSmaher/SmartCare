using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using paymentEntity = SmartCare.Domain.Entities.Payment;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class HandlePaymentAsyncHandler : IRequestHandler<HandlePaymentAsyncCommand, Unit>
    {
        #region Fields
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentGetway _paymentGateway;

        #endregion

        public HandlePaymentAsyncHandler(IPaymentRepository paymentRepository, IPaymentGetway paymentGateway)
        {
            _paymentRepository = paymentRepository;
            _paymentGateway = paymentGateway;
        }

        public async Task<Unit> Handle(HandlePaymentAsyncCommand request, CancellationToken cancellationToken)
        {
            var order = request.order;
            var payment = order.Payment;

            // No payment yet → create new
            if (payment == null)
            {
                //    var newPayment = new Payment
                //    {
                //        OrderId = order.Id,
                //        Amount = order.TotalPrice,
                //        Status = PaymentStatus.Pending,
                //        Version = 1,
                //        Method = PaymentMethod.Cash,
                //        CreatedAt = DateTime.UtcNow
                //    };

                //    await _paymentRepository.AddAsync(newPayment);

                //    order.PaymentIntentId = null;
                //    order.PaymentVersion = newPayment.Version;
                return Unit.Value;
            }

            // Pending → update amount + intent
            if (payment.Status == PaymentStatus.Pending)
            {
                payment.Amount = order.TotalPrice;
                payment.Version += 1;
                payment.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(payment.PaymentIntentId))
                {
                    await _paymentGateway
                        .UpdatePaymentIntentAmountAsync(
                            payment.PaymentIntentId,
                            order.TotalPrice);

                    order.PaymentIntentId = payment.PaymentIntentId;
                    order.PaymentVersion = payment.Version;
                }

                await _paymentRepository.UpdateAsync(payment);
                return Unit.Value;
            }

            // Paid / Failed → create new payment version
            var replacement = new paymentEntity
            {
                OrderId = order.Id,
                Amount = order.TotalPrice,
                Status = PaymentStatus.Pending,
                Version = payment.Version + 1,
                Method = PaymentMethod.Cash,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(replacement);

            order.PaymentIntentId = null;
            order.PaymentVersion = replacement.Version;

            return Unit.Value;
        }
    }
}
