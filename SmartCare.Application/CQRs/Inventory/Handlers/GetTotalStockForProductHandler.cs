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
    public class GetTotalStockForProductHandler : IRequestHandler<GetTotalStockForProductQuery, Response<int>>
    {

        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;


        #endregion

        public GetTotalStockForProductHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<Response<int>> Handle(GetTotalStockForProductQuery request, CancellationToken cancellationToken)
        {
            var productId = request.productId;
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return _responseHandler.Failed<int>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<int>(SystemMessages.INVALID_INPUT);

            var totalStock = await _inventoryRepository.GetTotalStockForProductAsync(productId);
            return _responseHandler.Success(totalStock);
        }
    }
}
