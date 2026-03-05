using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetTotalOrdersCountHandler : IRequestHandler<GetTotalOrdersCountAsyncQuery, Response<int>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        public GetTotalOrdersCountHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<int>> Handle(GetTotalOrdersCountAsyncQuery request, CancellationToken cancellationToken)
        {
            var storeId = request.storeId;
            var count = await _unitOfWork.Orders.GetTotalOrdersCountAsync(storeId);
            return _responseHandler.Success(count);
        }
    }
}