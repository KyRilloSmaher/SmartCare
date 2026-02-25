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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class GetAllCategorysForAdminHandler : IRequestHandler<GetAllCategorysForAdminAsyncQuery, Response<IEnumerable<CategoryResponseForAdminDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public GetAllCategorysForAdminHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<CategoryResponseForAdminDto>>> Handle(GetAllCategorysForAdminAsyncQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "categories_all_admin";

            try
            {
                var cached = await _redisCacheService.GetDataAsync<IEnumerable<CategoryResponseForAdminDto>>(cacheKey, tag);
                if (cached != null)
                    return _responseHandler.Success(cached);
            }
            catch (Exception) { }

            var categories = await _unitOfWork.Categories.GetAllAsync();

            var categoriesDto = _mapper.Map<IEnumerable<CategoryResponseForAdminDto>>(categories);

            await _redisCacheService.SetDataAsync(cacheKey, categoriesDto, tag, Time.Default);

            return _responseHandler.Success(categoriesDto);
        }
    }
}