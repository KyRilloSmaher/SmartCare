using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Commands.RestoreCategory
{
    public class RestoreCategoryCommandHandler : IRequestHandler<RestoreCategoryCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<RestoreCategoryCommandHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public RestoreCategoryCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            ILogger<RestoreCategoryCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(RestoreCategoryCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, true);
                if (category == null)
                    return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
                if (!category.IsDeleted)
                {
                    _logger.LogWarning("Trying to Restore an non Deleted Category");
                    return _responseHandler.Success(true, SystemMessages.ALREADY_ACTIVE);
                }
                // Restore category
                category.IsDeleted = false;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Category {CategoryId} restored successfully", request.Id);

                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                    _logger.LogInformation("Category cache cleared after restoring {CategoryId}", request.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear category cache after restoring {CategoryId}", request.Id);
                }

                return _responseHandler.Success(true, SystemMessages.RECORD_UPDATED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring category {CategoryId}", request.Id);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}