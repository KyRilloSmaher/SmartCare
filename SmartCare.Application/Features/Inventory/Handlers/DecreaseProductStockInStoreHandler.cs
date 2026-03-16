using AutoMapper;
using MediatR;
using SmartCare.Application.Features.Inventory.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Inventory.Handlers
{
    public class DecreaseProductStockInStoreHandler : IRequestHandler<DecreaseProductStockInStoreCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        #endregion
        public DecreaseProductStockInStoreHandler(IResponseHandler responseHandler, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(DecreaseProductStockInStoreCommand request, CancellationToken cancellationToken)
        {
            var inventory = await _unitOfWork.Inventories
                 .GetInventoryByStoreAndProductAsync(request.storeId, request.productId);

            if (inventory == null)
                return _responseHandler.NotFound<bool>(SystemMessages.INVENTORY_NOT_FOUND_IN_STORE);

            if (request.quantityToSubtract > inventory.AvailableStock)
                return _responseHandler.BadRequest<bool>(
                    $"{SystemMessages.INSUFFICIENT_AVAILABLE_STOCK} Available: {inventory.AvailableStock}, Requested: {request.quantityToSubtract}.");

            inventory.StockQuantity -= request.quantityToSubtract;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _responseHandler.Success(true, SystemMessages.INVENTORY_STOCK_DECREASED);
        }
    }
}
