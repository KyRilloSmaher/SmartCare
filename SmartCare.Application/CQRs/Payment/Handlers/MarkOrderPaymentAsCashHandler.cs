using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Commands;
using SmartCare.Application.CQRs.Payment.Extensions;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class MarkOrderPaymentAsCashHandler : IRequestHandler<MarkOrderPaymentAsCashCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly PaymentExtensions _paymentExtensions;

        public MarkOrderPaymentAsCashHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            PaymentExtensions paymentExtensions)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _paymentExtensions = paymentExtensions;
        }

        public async Task<Response<bool>> Handle(MarkOrderPaymentAsCashCommand request, CancellationToken cancellationToken)
        {
            var OrderId = request.OrderId;
            var order = await _unitOfWork.Orders.GetByIdAsync(OrderId, true);

            if (order == null)
                return _responseHandler.Failed<bool>(SystemMessages.ORDER_NOT_FOUND);

            if (order.Status != OrderStatus.Pending)
                return _responseHandler.BadRequest<bool>("Order is not payable.");

            order.Status = OrderStatus.Confirmed;
            // Set Payment as Cash (you might want to create a payment record here)

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var client = await _unitOfWork.UserManager.FindByIdAsync(order.ClientId);

            if (order.OrderType == OrderType.Online)
            {
                await _paymentExtensions.SendOrderConfirmationEmailAsync(order, client);
            }
            else
            {
                var pickupCode = RandomNumberGenerator
                                    .GetInt32(0, 1_000_000)
                                    .ToString("D7");

                await _unitOfWork.Orders.UpdatePickupCodeHashAsync(
                    order.Id,
                    _paymentExtensions.ComputeSha256(pickupCode));
                await _paymentExtensions.SendPickupEmailAsync(order, client, pickupCode, ((FromStoreOrder)order).StoreId);
            }

            return _responseHandler.Success(true);
        }
    }
}