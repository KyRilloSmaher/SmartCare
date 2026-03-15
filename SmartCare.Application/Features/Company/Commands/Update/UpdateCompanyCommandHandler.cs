using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Company.Commands;
using SmartCare.Application.Features.Company.Commands.Update;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Company.Handlers
{
    public class UpdateCompanyCommandHandler
        : IRequestHandler<UpdateCompanyCommand, Response<CompanyResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateCompanyCommandHandler> _logger;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public UpdateCompanyCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper,
            ILogger<UpdateCompanyCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<CompanyResponseDto>> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            var id = request.CompanyDto.Id;
            if (id == Guid.Empty)
                return _responseHandler.BadRequest<CompanyResponseDto>(SystemMessages.INVALID_INPUT);

            var Company = await _unitOfWork.Companies.GetByIdAsync(id, true);
            if (Company == null)
                return _responseHandler.Failed<CompanyResponseDto>(SystemMessages.NOT_FOUND);

            try
            {
                _logger.LogInformation("Updating Company {CompanyId}", id);

                _mapper.Map(request.CompanyDto, Company);


                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Company {CompanyId} updated in database", id);

                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                    _logger.LogInformation("Company cache cleared after updating {CompanyId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear Company cache for {CompanyId}", id);
                }

                var updatedCompanyDto = _mapper.Map<CompanyResponseDto>(Company);
                return _responseHandler.Success(updatedCompanyDto, SystemMessages.RECORD_UPDATED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Company {CompanyId}", id);
                return _responseHandler.Failed<CompanyResponseDto>(SystemMessages.FAILED);
            }
        }
    }
}