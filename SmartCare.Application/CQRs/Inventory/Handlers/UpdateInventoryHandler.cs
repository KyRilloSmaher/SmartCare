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
    public class UpdateInventoryHandler : IRequestHandler<UpdateInventoryAsyncCommand, Response<InventoryAdminResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;


        #endregion

        public UpdateInventoryHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<Response<InventoryAdminResponseDto>> Handle(UpdateInventoryAsyncCommand request, CancellationToken cancellationToken)
        {
            var inventoryDto = request.inventoryDto;
            if (inventoryDto.InventoryId == Guid.Empty)
                return _responseHandler.BadRequest<InventoryAdminResponseDto>(SystemMessages.INVALID_INPUT);

            var existinginventory = await _inventoryRepository.GetByIdAsync(inventoryDto.InventoryId);
            if (existinginventory == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.INVENTORY_NOT_FOUND);
            }

            _mapper.Map(inventoryDto, existinginventory);
            var updatedinventory = await _inventoryRepository.UpdateinventoryAsync(inventoryDto.InventoryId, inventoryDto.StockQuantity, inventoryDto.ReservedQuantity);
            await _inventoryRepository.SaveChangesAsync();
            var updatedInventoryDto = _mapper.Map<InventoryAdminResponseDto>(updatedinventory);
            return _responseHandler.Success(updatedInventoryDto, SystemMessages.RECORD_UPDATED);
        }
    }
}
