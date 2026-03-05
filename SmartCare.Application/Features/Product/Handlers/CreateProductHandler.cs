using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Product.Commands;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Product.Handlers
{
    public class CreateProductHandler : IRequestHandler<CreateProductAsyncCommand, Response<ProductResponseDtoForAdmin>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public CreateProductHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IImageUploaderService imageUploaderService,
            IRedisCacheService redisCacheService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }

        public async Task<Response<ProductResponseDtoForAdmin>> Handle(CreateProductAsyncCommand request, CancellationToken cancellationToken)
        {
            var ProductDto = request.ProductDto;
            List<string> uploadedImageUrls = new();

            try
            {
                // Upload images if provided
                if (ProductDto.Images is not null && ProductDto.Images.Any())
                {
                    var uploadResults = await _imageUploaderService.UploadMultipleImagesAsync(ProductDto.Images, ImageFolder.ProductImages);

                    // Check for any failed uploads
                    if (uploadResults == null || uploadResults.Any(r => r.Error != null || string.IsNullOrEmpty(r.Url?.ToString())))
                    {
                        // Delete any successfully uploaded images
                        foreach (var result in uploadResults.Where(r => r.Url != null))
                            await _imageUploaderService.DeleteImageByUrlAsync(result.Url.ToString());

                        return _responseHandler.Failed<ProductResponseDtoForAdmin>(SystemMessages.FILE_UPLOAD_FAILED);
                    }

                    uploadedImageUrls = uploadResults.Select(r => r.Url.ToString()).ToList();
                }

                var product = _mapper.Map<SmartCare.Domain.Entities.Product>(ProductDto);

                if (uploadedImageUrls.Any())
                {
                    product.Images = uploadedImageUrls
                        .Select(url => new ProductImage { Url = url })
                        .ToList();
                }

                var createdEntity = await _unitOfWork.Products.AddAsync(product);

                if (createdEntity is null)
                    throw new InvalidOperationException("Product creation failed.");

                // Save changes through UnitOfWork
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Clear cache
                await _redisCacheService.DeleteKeysByTag(tag);

                var createdProductDto = _mapper.Map<ProductResponseDtoForAdmin>(createdEntity);
                return _responseHandler.Success(createdProductDto, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                // Clean up uploaded images if something went wrong
                foreach (var url in uploadedImageUrls)
                    await _imageUploaderService.DeleteImageByUrlAsync(url);

                return _responseHandler.Failed<ProductResponseDtoForAdmin>(SystemMessages.FAILED);
            }
        }
    }
}