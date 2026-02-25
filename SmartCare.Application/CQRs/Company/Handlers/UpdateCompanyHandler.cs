using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Company.Commands;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Company.Handlers
{
    public class UpdateCompanyHandler : IRequestHandler<UpdateCompanyAsyncCommand, Response<CompanyResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public UpdateCompanyHandler(
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

        public async Task<Response<CompanyResponseDto>> Handle(UpdateCompanyAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            var CompanyDto = request.CompanyDto;

            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<CompanyResponseDto>(SystemMessages.INVALID_INPUT);

            var Company = await _unitOfWork.Companies.GetByIdAsync(Id, true);
            if (Company == null)
                return _responseHandler.NotFound<CompanyResponseDto>(SystemMessages.NOT_FOUND);

            _mapper.Map(CompanyDto, Company);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Clear cache
            await _redisCacheService.DeleteKeysByTag(tag);

            var updatedCompanyDto = _mapper.Map<CompanyResponseDto>(Company);
            return _responseHandler.Success(updatedCompanyDto, SystemMessages.RECORD_UPDATED);
        }
    }
}