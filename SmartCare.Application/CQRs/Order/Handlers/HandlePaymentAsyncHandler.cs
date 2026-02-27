using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using paymentEntity = SmartCare.Domain.Entities.Payment;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class HandlePaymentAsyncHandler : IRequestHandler<HandlePaymentAsyncCommand, Unit>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGetway _paymentGateway;
        #endregion

        public HandlePaymentAsyncHandler(
            IUnitOfWork unitOfWork,
            IPaymentGetway paymentGateway)
        {
            _unitOfWork = unitOfWork;
            _paymentGateway = paymentGateway;
        }

        public async Task<Unit> Handle(HandlePaymentAsyncCommand request, CancellationToken cancellationToken)
        {
            //var order = request.order;
            //var payment = order.Payment;

            //// No payment yet → create new
            //if (payment == null)
            //{
            //    //    var newPayment = new Payment
            //    //    {
            //    //        OrderId = order.Id,
            //    //        Amount = order.TotalPrice,
            //    //        Status = PaymentStatus.Pending,
            //    //        Version = 1,
            //    //        Method = PaymentMethod.Cash,
            //    //        CreatedAt = DateTime.UtcNow
            //    //    };

            //    //    await _unitOfWork.Payments.AddAsync(newPayment);
            //    //    await _unitOfWork.SaveChangesAsync(cancellationToken);

            //    //    order.PaymentIntentId = null;
            //    //    order.PaymentVersion = newPayment.Version;
            //    return Unit.Value;
            //}

            //// Pending → update amount + intent
            //if (payment.Status == PaymentStatus.Pending)
            //{
            //    payment.Amount = order.TotalPrice;
            //    payment.Version += 1;
            //    payment.UpdatedAt = DateTime.UtcNow;

            //    if (!string.IsNullOrEmpty(payment.PaymentIntentId))
            //    {
            //        await _paymentGateway
            //            .UpdatePaymentIntentAmountAsync(
            //                payment.PaymentIntentId,
            //                order.TotalPrice);

            //        order.PaymentIntentId = payment.PaymentIntentId;
            //        order.PaymentVersion = payment.Version;
            //    }
            //    await _unitOfWork.SaveChangesAsync(cancellationToken);
            //    return Unit.Value;
            //}

            //// Paid / Failed → create new payment version
            //var replacement = new paymentEntity
            //{
            //    OrderId = order.Id,
            //    Amount = order.TotalPrice,
            //    Status = PaymentStatus.Pending,
            //    Version = payment.Version + 1,
            //    Method = Domain.Enums.PaymentMethod.Cash,
            //    CreatedAt = DateTime.UtcNow
            //};

            //await _unitOfWork.Payments.AddAsync(replacement);

            //order.PaymentIntentId = null;
            //order.PaymentVersion = replacement.Version;
            //await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}