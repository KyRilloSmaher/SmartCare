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
    public class IncreaseProductStockInStoreHandler : IRequestHandler<IncreaseProductStockInStoreCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        #endregion

        public IncreaseProductStockInStoreHandler(IResponseHandler responseHandler, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(IncreaseProductStockInStoreCommand request, CancellationToken cancellationToken)
        {
            var inventory = await _unitOfWork.Inventories
                     .GetInventoryByStoreAndProductAsync(request.storeId, request.productId);

            if (inventory == null)
                return _responseHandler.NotFound<bool>(SystemMessages.INVENTORY_NOT_FOUND_IN_STORE);

            inventory.StockQuantity += request.quantityToAdd;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _responseHandler.Success(true, SystemMessages.INVENTORY_STOCK_INCREASED);
        }
    }
}
