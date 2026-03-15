using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Company.Commands.ChangeLogo;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Company.Handlers
{
    public class ChangeCompanyLogoHandler
        : IRequestHandler<ChangeCompanyLogoCommand, Response<string>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly ILogger<ChangeCompanyLogoHandler> _logger;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public ChangeCompanyLogoHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            ILogger<ChangeCompanyLogoHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _logger = logger;
        }

        public async Task<Response<string>> Handle(
            ChangeCompanyLogoCommand request,
            CancellationToken cancellationToken)
        {
            var image = request.CompanyDto.Image;
            var id = request.CompanyDto.Id;

            if (id == Guid.Empty)
                return _responseHandler.BadRequest<string>(SystemMessages.INVALID_INPUT);

            var Company = await _unitOfWork.Companies.GetByIdAsync(id, true);

            if (Company is null)
                return _responseHandler.Failed<string>(SystemMessages.NOT_FOUND);

            var oldImageUrl = Company.LogoUrl;

            try
            {
                _logger.LogInformation("Uploading new logo for Company {CompanyId}", id);

                var uploadResult = await _imageUploaderService
                    .UploadImageAsync(image, ImageFolder.BrandLogos);

                if (uploadResult.Error != null)
                {
                    _logger.LogWarning("Image upload failed for Company {CompanyId}", id);
                    return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
                }

                var newImageUrl = uploadResult.Url.ToString();

                Company.LogoUrl = newImageUrl;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Company logo updated in database for {CompanyId}", id);
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    try
                    {
                        await _imageUploaderService.DeleteImageByUrlAsync(oldImageUrl);
                        _logger.LogInformation("Old logo deleted for Company {CompanyId}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old image for Company {CompanyId}", id);
                    }
                }
                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear Company cache");
                }

                return _responseHandler.Success(newImageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while changing Company logo for {CompanyId}", id);
                throw;
            }
        }
    }
}