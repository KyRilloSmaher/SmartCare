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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Company.Handlers
{
    public class ChangeCompanyLogoHandler : IRequestHandler<ChangeCompanyLogoAsyncCommand, Response<string>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public ChangeCompanyLogoHandler(
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

        public async Task<Response<string>> Handle(ChangeCompanyLogoAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            var CompanyDto = request.CompanyDto;

            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<string>(SystemMessages.INVALID_INPUT);

            var Company = await _unitOfWork.Companies.GetByIdAsync(Id, true);
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
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
            }

            Company.LogoUrl = uploadResult.Url.ToString();
            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Clear cache
            await _redisCacheService.DeleteKeysByTag(tag);

            return _responseHandler.Success(Company.LogoUrl);
        }
    }
}