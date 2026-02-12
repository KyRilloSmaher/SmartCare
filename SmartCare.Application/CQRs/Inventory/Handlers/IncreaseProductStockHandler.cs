using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Commands;
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
    public class IncreaseProductStockHandler : IRequestHandler<IncreaseProductStockCommand, Response<InventoryAdminResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;


        #endregion

        public IncreaseProductStockHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<Response<InventoryAdminResponseDto>> Handle(IncreaseProductStockCommand request, CancellationToken cancellationToken)
        {
            var InventoryId = request.InventoryId;
            var quantityToAdd = request.quantityToAdd;
            var inventory = await _inventoryRepository.GetByIdAsync(InventoryId);
            if (inventory == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (InventoryId == Guid.Empty && quantityToAdd <= 0)
                return _responseHandler.BadRequest<InventoryAdminResponseDto>(SystemMessages.INVALID_INPUT);
            var Inventory = await _inventoryRepository.IncreaseProductStockAsync(InventoryId, quantityToAdd);
            if (Inventory == null)
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.NOT_FOUND);
            var inventoryDto = _mapper.Map<InventoryAdminResponseDto>(Inventory);
            return _responseHandler.Success(inventoryDto);
        }
    }
}
