using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Category.Commands.ChangeCategoryLogo;
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
    public class ChangeCategoryLogoHandler
        : IRequestHandler<ChangeCategoryLogoCommand, Response<string>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly ILogger<ChangeCategoryLogoHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public ChangeCategoryLogoHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            ILogger<ChangeCategoryLogoHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _logger = logger;
        }

        public async Task<Response<string>> Handle(
            ChangeCategoryLogoCommand request,
            CancellationToken cancellationToken)
        {
            var image = request.CategoryDto.Image;
            var id = request.CategoryDto.Id;

            if (id == Guid.Empty)
                return _responseHandler.BadRequest<string>(SystemMessages.INVALID_INPUT);

            var category = await _unitOfWork.Categories.GetByIdAsync(id, true);

            if (category is null)
                return _responseHandler.Failed<string>(SystemMessages.NOT_FOUND);

            var oldImageUrl = category.LogoUrl;

            try
            {
                _logger.LogInformation("Uploading new logo for category {CategoryId}", id);

                var uploadResult = await _imageUploaderService
                    .UploadImageAsync(image, ImageFolder.CategoryImages);

                if (uploadResult.Error != null)
                {
                    _logger.LogWarning("Image upload failed for category {CategoryId}", id);
                    return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
                }

                var newImageUrl = uploadResult.Url.ToString();

                category.LogoUrl = newImageUrl;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Category logo updated in database for {CategoryId}", id);
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    try
                    {
                        await _imageUploaderService.DeleteImageByUrlAsync(oldImageUrl);
                        _logger.LogInformation("Old logo deleted for category {CategoryId}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old image for category {CategoryId}", id);
                    }
                }
                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear category cache");
                }

                return _responseHandler.Success(newImageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while changing category logo for {CategoryId}", id);
                throw;
            }
        }
    }
}