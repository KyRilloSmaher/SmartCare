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
    public class CreateInventoryHandler : IRequestHandler<CreateInventoryAsyncCommand, Response<InventoryAdminResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public CreateInventoryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<InventoryAdminResponseDto>> Handle(CreateInventoryAsyncCommand request, CancellationToken cancellationToken)
        {
            var inventoryDto = request.inventoryDto;

            var product = await _unitOfWork.Products.GetByIdAsync(inventoryDto.ProductId);
            if (product == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }

            var store = await _unitOfWork.Stores.GetByIdAsync(inventoryDto.StoreId);
            if (store == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.STORE_NOT_FOUND);
            }

            var inventory = _mapper.Map<SmartCare.Domain.Entities.Inventory>(inventoryDto);
            var savedInventory = await _unitOfWork.Inventories.AddAsync(inventory);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var InventoryDto = _mapper.Map<InventoryAdminResponseDto>(savedInventory);
            return _responseHandler.Success(InventoryDto);
        }
    }
}