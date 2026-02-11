using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Company.Commands;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Company.Handlers
{
    public class CreateCompanyHandler : IRequestHandler<CreateCompanyAsyncCommand, Response<CompanyResponseForAdminDto>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICompanyRepository _CompanyRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Companies;

        #endregion
        public CreateCompanyHandler(
            IResponseHandler responseHandler,
            ICompanyRepository companyRepository,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _CompanyRepository = companyRepository;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }


        public async Task<Response<CompanyResponseForAdminDto>> Handle(CreateCompanyAsyncCommand request, CancellationToken cancellationToken)
        {
            var companyDto = request.companyDto;

            string? uploadedImageUrl = null;

            try
            {
                // Upload profile image if provided
                if (companyDto.Logo is not null)
                {
                    var uploadResult = await _imageUploaderService.UploadImageAsync(companyDto.Logo, ImageFolder.BrandLogos);

                    if (uploadResult.Error != null)
                        return _responseHandler.Failed<CompanyResponseForAdminDto>(SystemMessages.FILE_UPLOAD_FAILED);

                    uploadedImageUrl = uploadResult.Url.ToString();
                }

                await _CompanyRepository.BeginTransactionAsync();

                var company = _mapper.Map<SmartCare.Domain.Entities.Company>(companyDto);
                company.LogoUrl = uploadedImageUrl;

                // Add to repository
                var createdEntity = await _CompanyRepository.AddAsync(company);

                if (createdEntity is null)
                {
                    await _CompanyRepository.RollBackAsync();
                    return _responseHandler.Failed<CompanyResponseForAdminDto>(SystemMessages.FAILED);
                }

                // Commit changes
                await _CompanyRepository.SaveChangesAsync();
                await _CompanyRepository.CommitTransactionAsync();

                await _redisCacheService.DeleteKeysByTag(tag);

                var createdCompanyDto = _mapper.Map<CompanyResponseForAdminDto>(createdEntity);
                return _responseHandler.Success(createdCompanyDto, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                await _CompanyRepository.RollBackAsync();

                // Delete uploaded image if something went wrong
                if (!string.IsNullOrEmpty(uploadedImageUrl))
                    await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                return _responseHandler.Failed<CompanyResponseForAdminDto>(SystemMessages.FAILED);
            }
        }
    }
}
