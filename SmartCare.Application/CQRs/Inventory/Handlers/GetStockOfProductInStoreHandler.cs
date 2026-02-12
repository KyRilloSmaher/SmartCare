using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Queries;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Handlers
{
    public class GetStockOfProductInStoreHandler : IRequestHandler<GetStockOfProductInStoreQuery, Response<InventoryUserResponseDto>?>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;


        #endregion
        public GetStockOfProductInStoreHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<Response<InventoryUserResponseDto>?> Handle(GetStockOfProductInStoreQuery request, CancellationToken cancellationToken)
        {
            var productId = request.productId;
            var storeId = request.storeId;
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return _responseHandler.Failed<InventoryUserResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (productId == Guid.Empty && storeId == Guid.Empty)
                return _responseHandler.BadRequest<InventoryUserResponseDto>(SystemMessages.INVALID_INPUT);
            var inventory = await _inventoryRepository.GetStockOfProductInStore(productId, storeId);
            if (inventory == null)
                return _responseHandler.Failed<InventoryUserResponseDto>(SystemMessages.NOT_FOUND);
            var inventoryDto = _mapper.Map<InventoryUserResponseDto>(inventory);
            return _responseHandler.Success(inventoryDto);
        }
    }
}
