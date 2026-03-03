using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdAsyncQuery, Response<OrderResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public GetOrderByIdHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderResponseDto>> Handle(GetOrderByIdAsyncQuery request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;

            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return _responseHandler.NotFound<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);
            }

            var dto = _mapper.Map<OrderResponseDto>(order);
            return _responseHandler.Success(dto);
        }
    }
}