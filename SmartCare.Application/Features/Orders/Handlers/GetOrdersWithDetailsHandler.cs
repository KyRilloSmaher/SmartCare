using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Extentions;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class GetOrdersWithDetailsHandler : IRequestHandler<GetOrdersWithDetailsAsyncQuery, Response<PaginatedResult<OrderResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _redisCacheService;
        private readonly string tag = CacheConstants.Orders;
        #endregion

        public GetOrdersWithDetailsHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IRedisCacheService redisCacheService)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _redisCacheService = redisCacheService;
        }

        public async Task<Response<PaginatedResult<OrderResponseDto>>> Handle(GetOrdersWithDetailsAsyncQuery request, CancellationToken cancellationToken)
        {

            var pageNumber = request.PageNumber;
            var pageSize = request.PageSize;

            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<OrderResponseDto>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            string cacheKey = $"orders_all_p{pageNumber}_s{pageSize}";

            try
            {
                var cachedProducts = await _redisCacheService.GetDataAsync<PaginatedResult<OrderResponseDto>>(cacheKey, tag);

                if (cachedProducts != null)
                {
                    return _responseHandler.Success(cachedProducts);
                }
            }
            catch (Exception)
            {
                // cache errors
            }

            var query = await _unitOfWork.Orders.GetOrdersWithDetailsAsync();
            var projectedQuery = _mapper.ProjectTo<OrderResponseDto>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            if (paginatedResult != null)
            {
                await _redisCacheService.SetDataAsync(cacheKey, paginatedResult, tag, Time.Default);
            }

            return _responseHandler.Success(paginatedResult);

        }
    }
}