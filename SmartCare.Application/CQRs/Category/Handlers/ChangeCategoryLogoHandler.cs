using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Category.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class ChangeCategoryLogoHandler : IRequestHandler<ChangeCategoryLogoAsyncCommand, Response<string>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Categories;


        #endregion
        public ChangeCategoryLogoHandler(
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


        public async Task<Response<string>> Handle(ChangeCategoryLogoAsyncCommand request, CancellationToken cancellationToken)
        {
            var Image = request.CategoryDto.Image;
            var Id = request.Id;
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<string>(SystemMessages.INVALID_INPUT);
            var category = await _categoryRepository.GetByIdAsync(Id, true);
            if (category is null)
                return _responseHandler.Failed<string>(SystemMessages.NOT_FOUND);
            // Delete old image 
            var oldImageUrl = category.LogoUrl;
            var DeleteResult = await _imageUploaderService.DeleteImageByUrlAsync(oldImageUrl);
            if (!DeleteResult)
                return _responseHandler.Failed<string>(SystemMessages.FAILED);
            var uploadResult = await _imageUploaderService.UploadImageAsync(Image, ImageFolder.CategoryImages);
            if (uploadResult.Error != null)
            {
                await _categoryRepository.RollBackAsync();
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
            }
            category.LogoUrl = uploadResult.Url.ToString();
            var updateResult = await _categoryRepository.UpdateAsync(category);
            //change version company tag
            await _redisCacheService.DeleteKeysByTag(tag);
            return _responseHandler.Success(updateResult.LogoUrl);
        }
    }
}
