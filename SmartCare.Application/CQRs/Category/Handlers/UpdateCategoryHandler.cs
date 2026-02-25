using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Category.Commands;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryAsyncCommand, Response<CategoryResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public UpdateCategoryHandler(
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

        public async Task<Response<CategoryResponseDto>> Handle(UpdateCategoryAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<CategoryResponseDto>(SystemMessages.INVALID_INPUT);

            var category = await _unitOfWork.Categories.GetByIdAsync(Id, true);
            if (category == null)
                return _responseHandler.Failed<CategoryResponseDto>(SystemMessages.NOT_FOUND);

            _mapper.Map(request.CategoryDto, category);
            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Clear cache
            await _redisCacheService.DeleteKeysByTag(tag);

            var updatedCategoryDto = _mapper.Map<CategoryResponseDto>(category);
            return _responseHandler.Success(updatedCategoryDto, SystemMessages.RECORD_UPDATED);
        }
    }
}