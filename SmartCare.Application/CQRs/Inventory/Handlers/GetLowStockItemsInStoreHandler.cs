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
    public class GetLowStockItemsInStoreHandler : IRequestHandler<GetLowStockItemsInStoreAsyncQuery, Response<IEnumerable<InventoryAdminResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        #endregion

        public GetLowStockItemsInStoreHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }


        public async Task<Response<IEnumerable<InventoryAdminResponseDto>>> Handle(GetLowStockItemsInStoreAsyncQuery request, CancellationToken cancellationToken)
        {
            var threshold = request.threshold;
            var storeId = request.storeId;
            if (storeId == Guid.Empty && threshold < 0)
                return _responseHandler.BadRequest<IEnumerable<InventoryAdminResponseDto>>(SystemMessages.INVALID_INPUT);
            var inventories = await _inventoryRepository.GetLowStockItemsInStoreAsync(threshold, storeId);
            if (inventories == null)
                return _responseHandler.Failed<IEnumerable<InventoryAdminResponseDto>>(SystemMessages.NOT_FOUND);
            var InventoryDto = _mapper.Map<IEnumerable<InventoryAdminResponseDto>>(inventories);
            return _responseHandler.Success(InventoryDto);
        }
    }
}
