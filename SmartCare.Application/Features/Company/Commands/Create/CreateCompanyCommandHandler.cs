using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Company.Commands.Create;
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
    public class CreateCompanyCommandHandler
        : IRequestHandler<CreateCompanyCommand, Response<CompanyResponseForAdminDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<CreateCompanyCommandHandler> _logger;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public CreateCompanyCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IImageUploaderService imageUploaderService,
            IMapper mapper,
            IRedisCacheService redisCacheService,
            ILogger<CreateCompanyCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<Response<CompanyResponseForAdminDto>> Handle(
            CreateCompanyCommand request,
            CancellationToken cancellationToken)
        {
            string? uploadedImageUrl = null;

            try
            {
                if (request.companyDto.Logo != null)
                {
                    _logger.LogInformation("Uploading image for new Company {CompanyName}", request.companyDto.Name);

                    var uploadResult = await _imageUploaderService
                        .UploadImageAsync(request.companyDto.Logo, ImageFolder.BrandLogos);

                    if (uploadResult.Error != null)
                    {
                        _logger.LogWarning("Image upload failed for new Company {CompanyName}", request.companyDto.Name);
                        return _responseHandler.Failed<CompanyResponseForAdminDto>(SystemMessages.FILE_UPLOAD_FAILED);
                    }

                    uploadedImageUrl = uploadResult.Url.ToString();
                }

                var Company = _mapper.Map<SmartCare.Domain.Entities.Company>(request.companyDto);
                Company.LogoUrl = uploadedImageUrl;

                var createResult = await _unitOfWork.Companies.AddAsync(Company);

                if (createResult is null)
                {
                    _logger.LogError("Failed to create Company {CompanyName}", request.companyDto.Name);

                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                        await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                    return _responseHandler.Failed<CompanyResponseForAdminDto>(SystemMessages.FAILED);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Invalidate all cached Companies  
                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                    _logger.LogInformation("Company cache cleared after creating Company {CompanyName}", request.companyDto.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear Company cache after creating Company {CompanyName}", request.companyDto.Name);
                }

                var createdCompanyDto = _mapper.Map<CompanyResponseForAdminDto>(createResult);
                _logger.LogInformation("Company created successfully: {CompanyName}", request.companyDto.Name);

                return _responseHandler.Success(createdCompanyDto, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating Company {CompanyName}", request.companyDto.Name);

                if (!string.IsNullOrEmpty(uploadedImageUrl))
                    await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                return _responseHandler.Failed<CompanyResponseForAdminDto>(SystemMessages.FAILED);
            }
        }
    }
}