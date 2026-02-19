using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Company.Commands;
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

namespace SmartCare.Application.CQRs.Company.Handlers
{
    public class ChangeCompanyLogoHandler : IRequestHandler<ChangeCompanyLogoAsyncCommand, Response<string>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICompanyRepository _CompanyRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Companies;

        #endregion

        public ChangeCompanyLogoHandler(
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

        public async Task<Response<string>> Handle(ChangeCompanyLogoAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            var CompanyDto = request.CompanyDto;
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<string>(SystemMessages.INVALID_INPUT);
            var Company = await _CompanyRepository.GetByIdAsync(Id, true);
            if (Company is null)
                return _responseHandler.NotFound<string>(SystemMessages.NOT_FOUND);
            // Delete old image 
            var oldImageUrl = Company.LogoUrl;
            var DeleteResult = await _imageUploaderService.DeleteImageByUrlAsync(oldImageUrl);
            if (!DeleteResult)
                return _responseHandler.Failed<string>(SystemMessages.FAILED);
            var uploadResult = await _imageUploaderService.UploadImageAsync(CompanyDto.Image, ImageFolder.BrandLogos);
            if (uploadResult.Error != null)
            {
                await _CompanyRepository.RollBackAsync();
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
            }
            Company.LogoUrl = uploadResult.Url.ToString();
            var updateResult = await _CompanyRepository.UpdateAsync(Company);

            await _redisCacheService.DeleteKeysByTag(tag);
            return _responseHandler.Success(updateResult.LogoUrl);
        }
    }
}
