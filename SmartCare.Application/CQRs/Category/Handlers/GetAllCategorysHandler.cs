using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Category.Queries;
using SmartCare.Application.DTOs.Caregory.Response;
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
    public class GetAllCategorysHandler : IRequestHandler<GetAllCategorysAsyncQuery, Response<IEnumerable<CategoryResponseDto>>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Categories;



        #endregion
        public GetAllCategorysHandler(
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
        public async Task<Response<IEnumerable<CategoryResponseDto>>> Handle(GetAllCategorysAsyncQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "categories_all_client";

            try
            {
                var cached = await _redisCacheService.GetDataAsync<IEnumerable<CategoryResponseDto>>(cacheKey, tag);
                if (cached != null) return _responseHandler.Success(cached);
            }
            catch (Exception) { }

            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var categoriesDto = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);

            await _redisCacheService.SetDataAsync(cacheKey, categoriesDto, tag, Time.Default);

            return _responseHandler.Success(categoriesDto);
        }
    }
}
