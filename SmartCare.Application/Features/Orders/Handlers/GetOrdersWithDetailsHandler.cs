using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetOrdersWithDetailsHandler : IRequestHandler<GetOrdersWithDetailsAsyncQuery, Response<IEnumerable<OrderResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public GetOrdersWithDetailsHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<OrderResponseDto>>> Handle(GetOrdersWithDetailsAsyncQuery request, CancellationToken cancellationToken)
        {
            var orders = await _unitOfWork.Orders.GetOrdersWithDetailsAsync();
            var dto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(dto);
        }
    }
}