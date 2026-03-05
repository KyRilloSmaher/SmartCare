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
    public class DecreaseProductStockHandler : IRequestHandler<DecreaseProductStockCommand, Response<InventoryAdminResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public DecreaseProductStockHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<InventoryAdminResponseDto>> Handle(DecreaseProductStockCommand request, CancellationToken cancellationToken)
        {
            var InventoryId = request.InventoryId;
            var quantityToSubtract = request.quantityToSubtract;

            if (InventoryId == Guid.Empty || quantityToSubtract <= 0)
                return _responseHandler.BadRequest<InventoryAdminResponseDto>(SystemMessages.INVALID_INPUT);

            var inventory = await _unitOfWork.Inventories.GetByIdAsync(InventoryId);
            if (inventory == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.INVENTORY_NOT_FOUND);
            }

           inventory.StockQuantity -= quantityToSubtract;

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var inventoryDto = _mapper.Map<InventoryAdminResponseDto>(inventory);
            return _responseHandler.Success(inventoryDto);
        }
    }
}