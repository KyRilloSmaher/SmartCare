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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class ChangeCategoryLogoHandler : IRequestHandler<ChangeCategoryLogoAsyncCommand, Response<string>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public ChangeCategoryLogoHandler(
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

        public async Task<Response<string>> Handle(ChangeCategoryLogoAsyncCommand request, CancellationToken cancellationToken)
        {
            var Image = request.CategoryDto.Image;
            var Id = request.Id;

            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<string>(SystemMessages.INVALID_INPUT);

            var category = await _unitOfWork.Categories.GetByIdAsync(Id, true);
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
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
            }

            category.LogoUrl = uploadResult.Url.ToString();
            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Clear cache
            await _redisCacheService.DeleteKeysByTag(tag);

            return _responseHandler.Success(category.LogoUrl);
        }
    }
}