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
    public class GetOrderWithDetailsByIdHandler : IRequestHandler<GetOrderWithDetailsByIdAsyncQuery, Response<OrderResponseDto?>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        #endregion

        public GetOrderWithDetailsByIdHandler(IResponseHandler responseHandler, IOrderRepository orderRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }
        public async Task<Response<OrderResponseDto?>> Handle(GetOrderWithDetailsByIdAsyncQuery request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;
            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.BAD_REQUEST);

            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
            {
                return _responseHandler.NotFound<OrderResponseDto?>(SystemMessages.ORDER_NOT_FOUND);
            }

            var dto = _mapper.Map<OrderResponseDto?>(order);
            return _responseHandler.Success(dto);
        }
    }
}
