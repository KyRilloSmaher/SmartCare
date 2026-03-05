using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Handlers
{
    public class TransferStockHandler : IRequestHandler<TransferStockAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public TransferStockHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(TransferStockAsyncCommand request, CancellationToken cancellationToken)
        {
            var fromInventoryId = request.fromInventoryId;
            var toInventoryId = request.toInventoryId;
            var quantity = request.quantity;

            if (fromInventoryId == Guid.Empty || toInventoryId == Guid.Empty || quantity <= 0)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var fromInventory = await _unitOfWork.Inventories.GetByIdAsync(fromInventoryId);
            if (fromInventory == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.INVENTORY_NOT_FOUND);
            }

            var toInventory = await _unitOfWork.Inventories.GetByIdAsync(toInventoryId);
            if (toInventory == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.INVENTORY_NOT_FOUND);
            }

            var AvailableQuantity = fromInventory.StockQuantity - fromInventory.ReservedQuantity;

            if (quantity > AvailableQuantity)
                throw new InvalidOperationException($"Quantity exceeds available quantity");

            var result = await _unitOfWork.Inventories.TransferStockAsync(fromInventoryId, toInventoryId, quantity);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result ? _responseHandler.Success(true, SystemMessages.INVENTORY_UPDATED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
    }
}