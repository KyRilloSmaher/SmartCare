using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Features.Company.Commands.Restore;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;


namespace SmartCare.Application.Features.Company.Commands.RestoreCompany
{
    public class RestoreCompanyCommandHandler : IRequestHandler<RestoreCompanyCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<RestoreCompanyCommandHandler> _logger;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public RestoreCompanyCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            ILogger<RestoreCompanyCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(RestoreCompanyCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            try
            {
                var Company = await _unitOfWork.Companies.GetByIdAsync(request.Id, true);
                if (Company == null)
                    return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
                if (!Company.IsDeleted)
                {
                    _logger.LogWarning("Trying to Restore an non Deleted Company");
                    return _responseHandler.Success(true, SystemMessages.ALREADY_ACTIVE);
                }
                // Restore Company
                Company.IsDeleted = false;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Company {CompanyId} restored successfully", request.Id);

                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                    _logger.LogInformation("Company cache cleared after restoring {CompanyId}", request.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear Company cache after restoring {CompanyId}", request.Id);
                }

                return _responseHandler.Success(true, SystemMessages.RECORD_UPDATED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring Company {CompanyId}", request.Id);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}