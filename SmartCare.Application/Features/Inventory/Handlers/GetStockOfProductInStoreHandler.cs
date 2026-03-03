using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Queries;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Handlers
{
    public class GetStockOfProductInStoreHandler : IRequestHandler<GetStockOfProductInStoreQuery, Response<InventoryUserResponseDto>?>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public GetStockOfProductInStoreHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<InventoryUserResponseDto>?> Handle(GetStockOfProductInStoreQuery request, CancellationToken cancellationToken)
        {
            var productId = request.productId;
            var storeId = request.storeId;

            if (productId == Guid.Empty || storeId == Guid.Empty)
                return _responseHandler.BadRequest<InventoryUserResponseDto>(SystemMessages.INVALID_INPUT);

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return _responseHandler.Failed<InventoryUserResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }

            var inventory = await _unitOfWork.Inventories.GetStockOfProductInStoreAsync(productId, storeId);
            if (inventory == null)
                return _responseHandler.Failed<InventoryUserResponseDto>(SystemMessages.NOT_FOUND);

            var inventoryDto = _mapper.Map<InventoryUserResponseDto>(inventory);
            return _responseHandler.Success(inventoryDto);
        }
    }
}