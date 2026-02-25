using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Commands;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;
using paymentEntity = SmartCare.Domain.Entities.Payment;
using SmartCare.Domain.IRepositories;
using Stripe;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class CreateOrUpdatePaymentHandler : IRequestHandler<CreateOrUpdatePaymentAsyncCommand, Response<PaymentIntentResponse>>
    {
        private readonly IPaymentGetway _paymentGateway;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;

        public CreateOrUpdatePaymentHandler(
            IPaymentGetway paymentGateway,
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler)
        {
            _paymentGateway = paymentGateway;
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
        }

        public async Task<Response<PaymentIntentResponse>> Handle(CreateOrUpdatePaymentAsyncCommand request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(orderId);

            if (order == null)
                return _responseHandler.BadRequest<PaymentIntentResponse>("Order not found");

            if (order.Status != OrderStatus.Pending)
                return _responseHandler.BadRequest<PaymentIntentResponse>("Order not payable");

            PaymentIntent intent;

            if (string.IsNullOrEmpty(order.PaymentIntentId))
            {
                intent = await _paymentGateway.CreatePaymentIntentAsync(
                    order.TotalPrice,
                    order.Id.ToString(),
                    order.PaymentVersion);

                order.PaymentIntentId = intent.Id;

                await _unitOfWork.Payments.AddAsync(new paymentEntity
                {
                    OrderId = order.Id,
                    Amount = order.TotalPrice,
                    PaymentIntentId = intent.Id,
                    ClientSecret = intent.ClientSecret,
                    Version = order.PaymentVersion
                });
            }
            else
            {
                intent = await _paymentGateway.UpdatePaymentIntentAmountAsync(
                    order.PaymentIntentId,
                    order.TotalPrice);
            }


            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _responseHandler.Success(new PaymentIntentResponse
            {
                ClientSecret = intent.ClientSecret,
                PaymentIntentId = intent.Id,
                Amount = order.TotalPrice
            });
        }
    }
}