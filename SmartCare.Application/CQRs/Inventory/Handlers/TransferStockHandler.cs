using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Commands;
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
    public class TransferStockHandler : IRequestHandler<TransferStockAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;


        #endregion

        public TransferStockHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(TransferStockAsyncCommand request, CancellationToken cancellationToken)
        {
            var fromInventoryId = request.fromInventoryId;
            var toInventoryId = request.toInventoryId;
            var quantity = request.quantity;
            if (fromInventoryId == Guid.Empty && toInventoryId == Guid.Empty && quantity <= 0)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);
            var Frominventory = await _inventoryRepository.GetByIdAsync(fromInventoryId);
            if (Frominventory == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.INVENTORY_NOT_FOUND);
            }
            var Toinventory = await _inventoryRepository.GetByIdAsync(toInventoryId);
            if (Toinventory == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.INVENTORY_NOT_FOUND);
            }
            var AvailableQuantity = Frominventory.StockQuantity - Frominventory.ReservedQuantity;

            if (quantity > AvailableQuantity)
                throw new InvalidOperationException($"Quantity exceed from AvailbleQuantity");
            var result = await _inventoryRepository.TransferStockAsync(fromInventoryId, toInventoryId, quantity);
            return result ? _responseHandler.Success(true, SystemMessages.INVENTORY_UPDATED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
    }
}
