using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Queries.GetCategoryById
{
    public class GetCategoryByIdQueryHandler
        : IRequestHandler<GetCategoryByIdQuery, Response<CategoryResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCategoryByIdQueryHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public GetCategoryByIdQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            ILogger<GetCategoryByIdQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<CategoryResponseDto>> Handle(
            GetCategoryByIdQuery request,
            CancellationToken cancellationToken)
        {
            var id = request.Id;

            if (id == Guid.Empty)
                return _responseHandler.BadRequest<CategoryResponseDto>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"{CacheConstants.Category}_{id}";

            try
            {
                var cachedCategory = await _redisCacheService
                    .GetDataAsync<CategoryResponseDto>(cacheKey, tag);

                if (cachedCategory != null)
                    return _responseHandler.Success(cachedCategory);
            }
            catch
            {
                _logger.LogError("Error Occured Will Retrieving Catgeory By id through Cahce");
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if (category == null)
                return _responseHandler.Failed<CategoryResponseDto>(SystemMessages.NOT_FOUND);

            var categoryDto = _mapper.Map<CategoryResponseDto>(category);

            await _redisCacheService
                .SetDataAsync(cacheKey, categoryDto, tag, Time.Default);

            return _responseHandler.Success(categoryDto);
        }
    }
}