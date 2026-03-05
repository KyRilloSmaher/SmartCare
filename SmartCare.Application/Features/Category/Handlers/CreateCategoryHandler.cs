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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryAsyncCommand, Response<CategoryResponseForAdminDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public CreateCategoryHandler(
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

                var category = _mapper.Map<SmartCare.Domain.Entities.Category>(request.CategoryDto);
                category.LogoUrl = uploadedImageUrl;

                var createResult = await _unitOfWork.Categories.AddAsync(category);
                if (createResult is null)
                {
                    // Clean up uploaded image if category creation fails
                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                        await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                    return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FAILED);
                }

                // Save changes through UnitOfWork
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Clear cache
                await _redisCacheService.DeleteKeysByTag(tag);

                var createdCategoryDto = _mapper.Map<CategoryResponseForAdminDto>(createResult);
                return _responseHandler.Success(createdCategoryDto, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                // Clean up uploaded image if any error occurs
                if (!string.IsNullOrEmpty(uploadedImageUrl))
                    await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FAILED);
            }
        }
    }
}