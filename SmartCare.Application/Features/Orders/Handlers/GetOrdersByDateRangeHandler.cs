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
    public class GetOrdersByDateRangeHandler : IRequestHandler<GetOrdersByDateRangeAsyncQuery, Response<IEnumerable<OrderResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public GetOrdersByDateRangeHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> Handle(GetOrdersByDateRangeAsyncQuery request, CancellationToken cancellationToken)
        {
            var startDate = request.startDate;
            var endDate = request.endDate;
            var storeId = request.storeId;

            if (startDate > endDate)
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_DATE_RANGE);

            var orders = await _unitOfWork.Orders.GetOrdersByDateRangeAsync(startDate, endDate, storeId);
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }
    }
}