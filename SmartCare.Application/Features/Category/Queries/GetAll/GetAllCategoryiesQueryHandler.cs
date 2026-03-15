using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Queries.GetAll
{
    public class GetAllCategoryiesQueryHandler
        : IRequestHandler<GetAllCategoryiesQuery, Response<IEnumerable<CategoryResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllCategoryiesQueryHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public GetAllCategoryiesQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            ILogger<GetAllCategoryiesQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<IEnumerable<CategoryResponseDto>>> Handle(GetAllCategoryiesQuery request,CancellationToken cancellationToken)
        {
            string cacheKey = CacheConstants.CategoriesClient;
            IEnumerable<CategoryResponseDto>? cached = null;

            try
            {
                cached = await _redisCacheService
                    .GetDataAsync<IEnumerable<CategoryResponseDto>>(cacheKey, tag);
            }
            catch
            {
                _logger.LogError("Cache Service Error Occurred In Get All Categories For Admin QueryHandler.");
            }

            if (cached != null && cached.Any())
                return _responseHandler.Success(cached);

            var categories = await _unitOfWork
                                        .Categories
                                        .GetAllActiveCategoriesAsync();

            var categoriesDto = _mapper
                .Map<IEnumerable<CategoryResponseDto>>(categories);

            await _redisCacheService
                .SetDataAsync(cacheKey, categoriesDto, tag, Time.Default);

            return _responseHandler.Success(categoriesDto);
        }
    }
}