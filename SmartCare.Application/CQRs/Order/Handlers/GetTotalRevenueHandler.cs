using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetTotalRevenueHandler : IRequestHandler<GetTotalRevenueAsyncQuery, Response<decimal>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IOrderRepository _orderRepository;

        #endregion
        public GetTotalRevenueHandler(IResponseHandler responseHandler, IOrderRepository orderRepository)
        {
            _responseHandler = responseHandler;
            _orderRepository = orderRepository;
        }

        public async Task<Response<decimal>> Handle(GetTotalRevenueAsyncQuery request, CancellationToken cancellationToken)
        {
            var storeId = request.storeId;
            var revenue = await _orderRepository.GetTotalRevenueAsync(storeId);
            return _responseHandler.Success(revenue);
        }
    }
}
