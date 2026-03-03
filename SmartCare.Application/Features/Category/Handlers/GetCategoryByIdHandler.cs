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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdAsyncQuery, Response<CategoryResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public GetCategoryByIdHandler(
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

        public async Task<Response<CategoryResponseDto>> Handle(GetCategoryByIdAsyncQuery request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<CategoryResponseDto>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"category_{Id}";

            try
            {
                var cachedCategory = await _redisCacheService.GetDataAsync<CategoryResponseDto>(cacheKey, tag);
                if (cachedCategory != null)
                    return _responseHandler.Success(cachedCategory);
            }
            catch (Exception) { /* Fallback to DB */ }

            var category = await _unitOfWork.Categories.GetByIdAsync(Id);
            if (category == null)
                return _responseHandler.Failed<CategoryResponseDto>(SystemMessages.NOT_FOUND);

            var categoryDto = _mapper.Map<CategoryResponseDto>(category);

            await _redisCacheService.SetDataAsync(cacheKey, categoryDto, tag, Time.Default);

            return _responseHandler.Success(categoryDto);
        }
    }
}