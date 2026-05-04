

using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Extentions;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.Application.Features.Product.Queries.GetGLobelProductsStockLevel
{
    public class GetGLobelProductStockLevelQueryHandler : IRequestHandler<GetGLobelProductStockLevelQuery, Response<PaginatedResult<GLobelProductStockLevel>>>
    {
        private readonly IResponseHandler _responseHandler;
        public readonly IUnitOfWork _unitOfWork;
        public readonly ILogger<GetGLobelProductStockLevelQueryHandler> _logger;

        public GetGLobelProductStockLevelQueryHandler(IResponseHandler responseHandler, IUnitOfWork unitOfWork, ILogger<GetGLobelProductStockLevelQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<PaginatedResult<GLobelProductStockLevel>>> Handle(GetGLobelProductStockLevelQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;

            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<GLobelProductStockLevel>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            var query = _unitOfWork.Products.GetGlobalProductStockLevels();
            var paginatedResult = await query.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }
    }
}
