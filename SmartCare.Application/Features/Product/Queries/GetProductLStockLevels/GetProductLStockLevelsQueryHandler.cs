using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Cart.Extensions;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Queries.GetProductLStockLevels
{
    public class GetProductLStockLevelsQueryHandler : IRequestHandler<GetProductLStockLevelsQuery, Response<IEnumerable<ProductLevelInStore>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetProductLStockLevelsQueryHandler> _logger;

        public GetProductLStockLevelsQueryHandler(IUnitOfWork unitOfWork, IResponseHandler responseHandler, ILogger<GetProductLStockLevelsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<IEnumerable<ProductLevelInStore>>> Handle(GetProductLStockLevelsQuery request, CancellationToken cancellationToken)
        {
            var productExists = await _unitOfWork.Products.EnsureProductExistsAsync(request.ProductId);
            if (productExists is null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", request.ProductId);
                return _responseHandler.Failed<IEnumerable<ProductLevelInStore>>($"Product with ID {request.ProductId} not found.");
            }
            var productLevels = await _unitOfWork.Products.productLevelInStores(request.ProductId);
            if (productLevels == null || !productLevels.Any())
            {
                _logger.LogInformation("No stock levels found for product with ID {ProductId}", request.ProductId);
                return _responseHandler.Success<IEnumerable<ProductLevelInStore>>(new List<ProductLevelInStore>(), "No stock levels found for the specified product.");
            }
            return _responseHandler.Success<IEnumerable<ProductLevelInStore>>(productLevels);

        }
    }
}
