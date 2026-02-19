using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Commands;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;
using paymentEntity = SmartCare.Domain.Entities.Payment;
using SmartCare.Domain.IRepositories;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class CreateOrUpdatePaymentHandler : IRequestHandler<CreateOrUpdatePaymentAsyncCommand, Response<PaymentIntentResponse>>
    {
        private readonly IPaymentGetway _paymentGateway;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IResponseHandler _responseHandler;

        public CreateOrUpdatePaymentHandler(IPaymentGetway paymentGateway, IPaymentRepository paymentRepository, IOrderRepository orderRepository, IResponseHandler responseHandler)
        {
            _paymentGateway = paymentGateway;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _responseHandler = responseHandler;
        }

        public async Task<Response<PaymentIntentResponse>> Handle(CreateOrUpdatePaymentAsyncCommand request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
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

                await _paymentRepository.AddAsync(new paymentEntity
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

            await _orderRepository.UpdateAsync(order);


            return _responseHandler.Success(new PaymentIntentResponse
            {
                ClientSecret = intent.ClientSecret,
                PaymentIntentId = intent.Id,
                Amount = order.TotalPrice
            });
        }
    }
}
