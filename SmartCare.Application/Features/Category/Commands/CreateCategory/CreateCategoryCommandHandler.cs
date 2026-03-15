using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Category.Commands.CreateCategory;
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
    public class CreateCategoryCommandHandler
        : IRequestHandler<CreateCategoryCommand, Response<CategoryResponseForAdminDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public CreateCategoryCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IImageUploaderService imageUploaderService,
            IMapper mapper,
            IRedisCacheService redisCacheService,
            ILogger<CreateCategoryCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<Response<CategoryResponseForAdminDto>> Handle(
            CreateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            string? uploadedImageUrl = null;

            try
            {
                if (request.CategoryDto.Logo != null)
                {
                    _logger.LogInformation("Uploading image for new category {CategoryName}", request.CategoryDto.Name);

                    var uploadResult = await _imageUploaderService
                        .UploadImageAsync(request.CategoryDto.Logo, ImageFolder.CategoryImages);

                    if (uploadResult.Error != null)
                    {
                        _logger.LogWarning("Image upload failed for new category {CategoryName}", request.CategoryDto.Name);
                        return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FILE_UPLOAD_FAILED);
                    }

                    uploadedImageUrl = uploadResult.Url.ToString();
                }

                var category = _mapper.Map<SmartCare.Domain.Entities.Category>(request.CategoryDto);
                category.LogoUrl = uploadedImageUrl;

                var createResult = await _unitOfWork.Categories.AddAsync(category);

                if (createResult is null)
                {
                    _logger.LogError("Failed to create category {CategoryName}", request.CategoryDto.Name);

                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                        await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                    return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FAILED);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Invalidate all cached categories
                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                    _logger.LogInformation("Category cache cleared after creating category {CategoryName}", request.CategoryDto.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear category cache after creating category {CategoryName}", request.CategoryDto.Name);
                }

                var createdCategoryDto = _mapper.Map<CategoryResponseForAdminDto>(createResult);
                _logger.LogInformation("Category created successfully: {CategoryName}", request.CategoryDto.Name);

                return _responseHandler.Success(createdCategoryDto, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating category {CategoryName}", request.CategoryDto.Name);

                if (!string.IsNullOrEmpty(uploadedImageUrl))
                    await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FAILED);
            }
        }
    }
}