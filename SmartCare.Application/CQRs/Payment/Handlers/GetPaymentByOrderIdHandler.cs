using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Queries;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using PaymentEntity = SmartCare.Domain.Entities.Payment;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class GetPaymentByOrderIdHandler : IRequestHandler<GetPaymentByOrderIdAsyncQuery, PaymentEntity?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPaymentByOrderIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<PaymentEntity?> Handle(GetPaymentByOrderIdAsyncQuery request, CancellationToken cancellationToken)
            => _unitOfWork.Payments.GetByOrderIdAsync(request.orderId);
    }
}