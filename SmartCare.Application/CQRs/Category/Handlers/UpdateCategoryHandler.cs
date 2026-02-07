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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryAsyncCommand, Response<CategoryResponseDto>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Categories;

        #endregion
        public UpdateCategoryHandler(
            IResponseHandler responseHandler,
            ICategoryRepository categoryRepository,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _categoryRepository = categoryRepository;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }
        public async Task<Response<CategoryResponseDto>> Handle(UpdateCategoryAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<CategoryResponseDto>(SystemMessages.INVALID_INPUT);
            var category = await _categoryRepository.GetByIdAsync(Id, true);
            if (category == null)
                return _responseHandler.Failed<CategoryResponseDto>(SystemMessages.NOT_FOUND);
            _mapper.Map(request.CategoryDto, category);
            var updatedCategory = await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();
            //change version category tag
            await _redisCacheService.DeleteKeysByTag(tag);
            var updatedCategoryDto = _mapper.Map<CategoryResponseDto>(updatedCategory);
            return _responseHandler.Success(updatedCategoryDto, SystemMessages.RECORD_UPDATED);
        }
    }
}
