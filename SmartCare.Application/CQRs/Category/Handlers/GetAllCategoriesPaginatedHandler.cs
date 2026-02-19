using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Category.Queries;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.Extentions;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class GetAllCategoriesPaginatedHandler : IRequestHandler<GetAllCategoriesPaginatedAsyncQuery, Response<PaginatedResult<CategoryResponseDto>>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Categories;


        #endregion
        public GetAllCategoriesPaginatedHandler(
            IResponseHandler responseHandler,
            ICategoryRepository categoryRepository,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _categoryRepository = categoryRepository;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }


        public async Task<Response<PaginatedResult<CategoryResponseDto>>> Handle(GetAllCategoriesPaginatedAsyncQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<CategoryResponseDto>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            string cacheKey = $"categories_all_p{pageNumber}_s{pageSize}";

            try
            {
                var cached = await _redisCacheService.GetDataAsync<PaginatedResult<CategoryResponseDto>>(cacheKey, tag);
                if (cached != null) return _responseHandler.Success(cached);
            }
            catch (Exception) { }

            var query = _categoryRepository.GetAllCategoriesQuerable();
            var projectedQuery = _mapper.ProjectTo<CategoryResponseDto>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            if (paginatedResult != null)
            {
                await _redisCacheService.SetDataAsync(cacheKey, paginatedResult, tag, Time.Default);
            }

            return _responseHandler.Success(paginatedResult);
        }
    }
}
