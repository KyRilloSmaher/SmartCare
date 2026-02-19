using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetOrderCountByStatusHandler : IRequestHandler<GetOrderCountByStatusAsyncQuery, Response<Dictionary<OrderStatus, int>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IOrderRepository _orderRepository;

        #endregion

        public GetOrderCountByStatusHandler(IResponseHandler responseHandler, IOrderRepository orderRepository)
        {
            _responseHandler = responseHandler;
            _orderRepository = orderRepository;
        }

        public async Task<Response<Dictionary<OrderStatus, int>>> Handle(GetOrderCountByStatusAsyncQuery request, CancellationToken cancellationToken)
        {
            var storeId = request.storeId;
            var counts = await _orderRepository.GetOrderCountByStatusAsync(storeId);
            return _responseHandler.Success(counts);
        }
    }
}
