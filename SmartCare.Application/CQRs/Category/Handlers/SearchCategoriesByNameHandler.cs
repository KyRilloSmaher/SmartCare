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
    public class SearchCategoriesByNameHandler : IRequestHandler<SearchCategoriesByNameAsyncQuery, Response<IEnumerable<CategoryResponseDto>>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Categories;



        #endregion
        public SearchCategoriesByNameHandler(
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


        public async Task<Response<IEnumerable<CategoryResponseDto>>> Handle(SearchCategoriesByNameAsyncQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.SearchCategoryByNameAsync(request.name);
            var categoriesDto = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
            return _responseHandler.Success(categoriesDto);
        }
    }
}
