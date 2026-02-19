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
using SmartCare.Domain.Entities;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Handlers
{
    public class CreateInventoryHandler : IRequestHandler<CreateInventoryAsyncCommand, Response<InventoryAdminResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;


        #endregion

        public CreateInventoryHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<Response<InventoryAdminResponseDto>> Handle(CreateInventoryAsyncCommand request, CancellationToken cancellationToken)
        {
            var inventoryDto = request.inventoryDto;
            var product = await _productRepository.GetByIdAsync(inventoryDto.ProductId);
            if (product == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }

            var store = await _storeRepository.GetByIdAsync(inventoryDto.StoreId);
            if (store == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.STORE_NOT_FOUND);
            }
            var inventory = _mapper.Map<SmartCare.Domain.Entities.Inventory>(inventoryDto);
            var savedInventory = await _inventoryRepository.AddAsync(inventory);
            var InventoryDto = _mapper.Map<InventoryAdminResponseDto>(savedInventory);
            return _responseHandler.Success(InventoryDto);
        }
    }
}
