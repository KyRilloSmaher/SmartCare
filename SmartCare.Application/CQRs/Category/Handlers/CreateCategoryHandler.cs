using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Category.Commands;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryAsyncCommand, Response<CategoryResponseForAdminDto>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Categories;

        #endregion
        public CreateCategoryHandler(
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
        public async Task<Response<CategoryResponseForAdminDto>> Handle(CreateCategoryAsyncCommand request, CancellationToken cancellationToken)
        {
            string? uploadedImageUrl = null;
            var Logo = request.CategoryDto.Logo;
            try
            {
                if (Logo is not null)
                {
                    var uploadResult = await _imageUploaderService.UploadImageAsync(Logo, ImageFolder.CategoryImages);

                    if (uploadResult.Error != null)
                        return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FILE_UPLOAD_FAILED);

                    uploadedImageUrl = uploadResult.Url.ToString();
                }

                await _categoryRepository.BeginTransactionAsync();

                var category = _mapper.Map<SmartCare.Domain.Entities.Category>(request.CategoryDto);
                category.LogoUrl = uploadedImageUrl;

                var createResult = await _categoryRepository.AddAsync(category);
                if (createResult is null)
                {
                    await _categoryRepository.RollBackAsync();
                    return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FAILED);
                }

                await _categoryRepository.CommitTransactionAsync();

                await _redisCacheService.DeleteKeysByTag(tag);
                var createdCategoryDto = _mapper.Map<CategoryResponseForAdminDto>(createResult);
                return _responseHandler.Success(createdCategoryDto, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                await _categoryRepository.RollBackAsync();

                if (!string.IsNullOrEmpty(uploadedImageUrl))
                    await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FAILED);
            }
        }
    }
}
