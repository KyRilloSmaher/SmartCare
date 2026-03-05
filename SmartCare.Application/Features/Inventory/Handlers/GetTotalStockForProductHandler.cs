using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Handlers
{
    public class GetTotalStockForProductHandler : IRequestHandler<GetTotalStockForProductQuery, Response<int>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public GetTotalStockForProductHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<int>> Handle(GetTotalStockForProductQuery request, CancellationToken cancellationToken)
        {
            var productId = request.productId;

            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<int>(SystemMessages.INVALID_INPUT);

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return _responseHandler.Failed<int>(SystemMessages.PRODUCT_NOT_FOUND);
            }

            var totalStock = await _unitOfWork.Inventories.GetTotalStockForProductAsync(productId);
            return _responseHandler.Success(totalStock);
        }
    }
}