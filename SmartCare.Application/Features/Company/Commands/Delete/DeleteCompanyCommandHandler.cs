using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Company.Commands.Delete;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Company.Commands
{
    public class DeleteCompanyCommandHandler
        : IRequestHandler<DeleteCompanyCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly ILogger<DeleteCompanyCommandHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public DeleteCompanyCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            ILogger<DeleteCompanyCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var Company = await _unitOfWork.Companies.GetByIdAsync(request.Id);
            if (Company == null)
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);

            try
            {
                if (!string.IsNullOrEmpty(Company.LogoUrl))
                {
                    try
                    {
                        var deleteResult = await _imageUploaderService.DeleteImageByUrlAsync(Company.LogoUrl);
                        if (!deleteResult)
                            _logger.LogWarning("Failed to delete Company logo for {CompanyId}", request.Id);
                        else
                            _logger.LogInformation("Deleted Company logo for {CompanyId}", request.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deleting Company logo for {CompanyId}", request.Id);
                    }
                }
                await _unitOfWork.Companies.DeleteAsync(Company);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Clear related cache
                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                    _logger.LogInformation("Cleared Company cache after deleting {CompanyId}", request.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear Company cache after deleting {CompanyId}", request.Id);
                }

                _logger.LogInformation("Company {CompanyId} deleted successfully", request.Id);
                return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Company {CompanyId}", request.Id);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}