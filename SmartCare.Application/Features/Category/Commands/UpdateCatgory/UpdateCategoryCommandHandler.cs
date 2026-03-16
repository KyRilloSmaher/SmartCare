using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Category.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class UpdateCategoryCommandHandler
        : IRequestHandler<UpdateCategoryCommand, Response<CategoryResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateCategoryCommandHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public UpdateCategoryCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper,
            ILogger<UpdateCategoryCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<CategoryResponseDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var id = request.CategoryDto.Id;
            if (id == Guid.Empty)
                return _responseHandler.BadRequest<CategoryResponseDto>(SystemMessages.INVALID_INPUT);

            var category = await _unitOfWork.Categories.GetByIdAsync(id, true);
            if (category == null)
                return _responseHandler.Failed<CategoryResponseDto>(SystemMessages.NOT_FOUND);

            try
            {
                _logger.LogInformation("Updating category {CategoryId}", id);

                _mapper.Map(request.CategoryDto, category);


                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Category {CategoryId} updated in database", id);

                try
                {
                    await _redisCacheService.DeleteKeysByTag(tag);
                    _logger.LogInformation("Category cache cleared after updating {CategoryId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clear category cache for {CategoryId}", id);
                }

                var updatedCategoryDto = _mapper.Map<CategoryResponseDto>(category);
                return _responseHandler.Success(updatedCategoryDto, SystemMessages.RECORD_UPDATED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return _responseHandler.Failed<CategoryResponseDto>(SystemMessages.FAILED);
            }
        }
    }
}