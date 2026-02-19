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
    public class GetAvailableInventoriesForProductHandler : IRequestHandler<GetAvailableInventoriesForProductQuery, Response<IEnumerable<InventoryUserResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        #endregion

        public GetAvailableInventoriesForProductHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }


        public async Task<Response<IEnumerable<InventoryUserResponseDto>>> Handle(GetAvailableInventoriesForProductQuery request, CancellationToken cancellationToken)
        {
            var productId = request.productId;
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return _responseHandler.Failed<IEnumerable<InventoryUserResponseDto>>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<IEnumerable<InventoryUserResponseDto>>(SystemMessages.INVALID_INPUT);
            var inventories = await _inventoryRepository.GetAvailableInventoriesForProductAsync(productId);
            if (inventories == null)
                return _responseHandler.Failed<IEnumerable<InventoryUserResponseDto>>(SystemMessages.NOT_FOUND);
            var inventoryDtoList = _mapper.Map<IEnumerable<InventoryUserResponseDto>>(inventories.ToList());
            return _responseHandler.Success(inventoryDtoList);
        }
    }
}
