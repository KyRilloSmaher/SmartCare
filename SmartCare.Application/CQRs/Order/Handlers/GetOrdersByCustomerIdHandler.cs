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
    public class GetOrdersByCustomerIdHandler : IRequestHandler<GetOrdersByCustomerIdAsyncQuery, Response<IEnumerable<OrderResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        #endregion

        public GetOrdersByCustomerIdHandler(IResponseHandler responseHandler, IClientRepository clientRepository, IOrderRepository orderRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> Handle(GetOrdersByCustomerIdAsyncQuery request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            if (string.IsNullOrWhiteSpace(clientId))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.BAD_REQUEST);

            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.USER_NOT_FOUND);
            }
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(clientId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }
    }
}
