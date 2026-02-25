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
    public class SetStockLevelHandler : IRequestHandler<SetStockLevelAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public SetStockLevelHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(SetStockLevelAsyncCommand request, CancellationToken cancellationToken)
        {
            var inventoryId = request.inventoryId;
            var newQuantity = request.newQuantity;

            if (inventoryId == Guid.Empty || newQuantity <= 0)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var result = await _unitOfWork.Inventories.SetStockLevelAsync(inventoryId, newQuantity);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result ? _responseHandler.Success(true, SystemMessages.INVENTORY_UPDATED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
    }
}