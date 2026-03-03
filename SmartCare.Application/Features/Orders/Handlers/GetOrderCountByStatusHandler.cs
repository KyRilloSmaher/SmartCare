using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetOrderCountByStatusHandler : IRequestHandler<GetOrderCountByStatusAsyncQuery, Response<Dictionary<OrderStatus, int>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        public GetOrderCountByStatusHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<Dictionary<OrderStatus, int>>> Handle(GetOrderCountByStatusAsyncQuery request, CancellationToken cancellationToken)
        {
            var storeId = request.storeId;
            var counts = await _unitOfWork.Orders.GetOrderCountByStatusAsync(storeId);
            return _responseHandler.Success(counts);
        }
    }
}