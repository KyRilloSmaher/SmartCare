using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Commands;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Handlers
{
    public class UpdateInventoryHandler : IRequestHandler<UpdateInventoryAsyncCommand, Response<InventoryAdminResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public UpdateInventoryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<InventoryAdminResponseDto>> Handle(UpdateInventoryAsyncCommand request, CancellationToken cancellationToken)
        {
            var inventoryDto = request.inventoryDto;

            if (inventoryDto.InventoryId == Guid.Empty)
                return _responseHandler.BadRequest<InventoryAdminResponseDto>(SystemMessages.INVALID_INPUT);

            var existingInventory = await _unitOfWork.Inventories.GetByIdAsync(inventoryDto.InventoryId);
            if (existingInventory == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.INVENTORY_NOT_FOUND);
            }

            _mapper.Map(inventoryDto, existingInventory);
            //var updatedInventory = await _unitOfWork.Inventories.UpdateinventoryAsync(
            //    inventoryDto.InventoryId,
            //    inventoryDto.StockQuantity,
            //    inventoryDto.ReservedQuantity);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedInventoryDto = _mapper.Map<InventoryAdminResponseDto>(existingInventory);
            return _responseHandler.Success(updatedInventoryDto, SystemMessages.RECORD_UPDATED);
        }
    }
}