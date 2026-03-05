using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetOrdersByCustomerIdHandler : IRequestHandler<GetOrdersByCustomerIdAsyncQuery, Response<IEnumerable<OrderResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public GetOrdersByCustomerIdHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> Handle(GetOrdersByCustomerIdAsyncQuery request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;

            if (string.IsNullOrWhiteSpace(clientId))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.BAD_REQUEST);

            var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
            if (client == null)
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.USER_NOT_FOUND);
            }

            var orders = await _unitOfWork.Orders.GetOrdersByCustomerIdAsync(clientId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }
    }
}