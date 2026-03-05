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
    public class GetTotalRevenueHandler : IRequestHandler<GetTotalRevenueAsyncQuery, Response<decimal>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        public GetTotalRevenueHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<decimal>> Handle(GetTotalRevenueAsyncQuery request, CancellationToken cancellationToken)
        {
            var storeId = request.storeId;
            var revenue = await _unitOfWork.Orders.GetTotalRevenueAsync(storeId);
            return _responseHandler.Success(revenue);
        }
    }
}