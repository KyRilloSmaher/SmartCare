using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetTopNOrdersByValueHandler : IRequestHandler<GetTopNOrdersByValueAsyncQuery, Response<IEnumerable<OrderResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        #endregion

        public GetTopNOrdersByValueHandler(IResponseHandler responseHandler, IOrderRepository orderRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> Handle(GetTopNOrdersByValueAsyncQuery request, CancellationToken cancellationToken)
        {
            var n = request.n;
            var storeId = request.storeId;
            if (n <= 0)
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_INPUT);

            var orders = await _orderRepository.GetTopNOrdersByValueAsync(n, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }
    }
}
