using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetOrdersByCustomerAndStatusHandler : IRequestHandler<GetOrdersByCustomerAndStatusAsyncQuery, Response<IEnumerable<OrderResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public GetOrdersByCustomerAndStatusHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> Handle(GetOrdersByCustomerAndStatusAsyncQuery request, CancellationToken cancellationToken)
        {
            var customerId = request.customerId;
            var status = request.status;

            if (string.IsNullOrWhiteSpace(customerId))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.BAD_REQUEST);

            if (!Enum.IsDefined(typeof(OrderStatus), status))
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_ORDER_STATUS);

            var orders = await _unitOfWork.Orders.GetOrdersByCustomerAndStatusAsync(customerId, status);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }
    }
}