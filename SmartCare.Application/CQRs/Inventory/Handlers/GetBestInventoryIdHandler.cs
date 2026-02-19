using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Queries;
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
    public class GetBestInventoryIdHandler : IRequestHandler<GetBestInventoryIdQuery, Response<Guid>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;


        #endregion

        public GetBestInventoryIdHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<Response<Guid>> Handle(GetBestInventoryIdQuery request, CancellationToken cancellationToken)
        {
            var productId = request.productId;
            var  quantityRequired = request.quantityRequired;
            if (productId == Guid.Empty && quantityRequired <= 0)
                return _responseHandler.BadRequest<Guid>(SystemMessages.INVALID_INPUT);
            var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(productId, quantityRequired);
            if (inventoryId == Guid.Empty)
                return _responseHandler.Failed<Guid>(SystemMessages.NOT_FOUND);
            return _responseHandler.Success(inventoryId);
        }
    }
}
