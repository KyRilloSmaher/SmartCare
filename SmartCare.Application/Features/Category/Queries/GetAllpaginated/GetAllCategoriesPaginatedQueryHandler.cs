using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.Extentions;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Queries.GetAllpaginated
{
    public class GetAllCategoriesPaginatedQueryHandler
        : IRequestHandler<GetAllCategoriesPaginatedQuery, Response<PaginatedResult<CategoryResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllCategoriesPaginatedQueryHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public GetAllCategoriesPaginatedQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            ILogger<GetAllCategoriesPaginatedQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<PaginatedResult<CategoryResponseDto>>> Handle(
            GetAllCategoriesPaginatedQuery request,
            CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;

            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<CategoryResponseDto>>(
                    SystemMessages.INVALID_PAGINATION_PARAMETERS);

            string cacheKey = $"{CacheConstants.CategoriesClient}_p{pageNumber}_s{pageSize}";

            try
            {
                var cached = await _redisCacheService
                    .GetDataAsync<PaginatedResult<CategoryResponseDto>>(cacheKey, tag);

                if (cached != null)
                    return _responseHandler.Success(cached);
            }
            catch
            {
                _logger.LogError("Cache Service Error Occurred In Get All Categories QueryHandler.");
            }

            var query = _unitOfWork.Categories
                .GetCategoriesQueryable();

            var projectedQuery = _mapper
                .ProjectTo<CategoryResponseDto>(query);

            var paginatedResult = await projectedQuery
                .ToPaginatedListAsync(pageNumber, pageSize);

            await _redisCacheService
                .SetDataAsync(cacheKey, paginatedResult, tag, Time.Default);

            return _responseHandler.Success(paginatedResult);
        }
    }
}