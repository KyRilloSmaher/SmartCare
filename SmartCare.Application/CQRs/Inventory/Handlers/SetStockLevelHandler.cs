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
    public class SetStockLevelHandler : IRequestHandler<SetStockLevelAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;


        #endregion
        public SetStockLevelHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(SetStockLevelAsyncCommand request, CancellationToken cancellationToken)
        {
            var inventoryId = request.inventoryId;
            var newQuantity = request.newQuantity;
            if (inventoryId == Guid.Empty && newQuantity <= 0)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var result = await _inventoryRepository.SetStockLevelAsync(inventoryId, newQuantity);
            return result ? _responseHandler.Success(true, SystemMessages.INVENTORY_UPDATED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
    }
}
