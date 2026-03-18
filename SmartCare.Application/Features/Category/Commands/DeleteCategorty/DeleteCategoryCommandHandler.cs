using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Commands
{
    public class DeleteCategoryCommandHandler
        : IRequestHandler<DeleteCategoryCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly ILogger<DeleteCategoryCommandHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public DeleteCategoryCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            ILogger<DeleteCategoryCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var category = await _unitOfWork.Categories.GetByIdAsync(request.Id);
            if (category == null)
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);

            try
            {
                if (!string.IsNullOrEmpty(category.LogoUrl))
                {
                    try
                    {
                        var deleteResult = await _imageUploaderService.DeleteImageByUrlAsync(category.LogoUrl);
                        if (!deleteResult)
                            _logger.LogWarning("Failed to delete category logo for {CategoryId}", request.Id);
                        else
                            _logger.LogInformation("Deleted category logo for {CategoryId}", request.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deleting category logo for {CategoryId}", request.Id);
                    }
                }
                await _unitOfWork.Categories.DeleteAsync(category);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Clear related cache
                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                    _logger.LogInformation("Cleared category cache after deleting {CategoryId}", request.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear category cache after deleting {CategoryId}", request.Id);
                }

                _logger.LogInformation("Category {CategoryId} deleted successfully", request.Id);
                return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", request.Id);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}