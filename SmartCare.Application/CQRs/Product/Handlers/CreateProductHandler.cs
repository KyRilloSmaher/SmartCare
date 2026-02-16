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
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Product.Handlers
{
    public class CreateProductHandler : IRequestHandler<CreateProductAsyncCommand, Response<ProductResponseDtoForAdmin>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IProductRepository _productRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Products;

        #endregion

        public CreateProductHandler(IResponseHandler responseHandler, IProductRepository productRepository, IImageUploaderService imageUploaderService, IRedisCacheService redisCacheService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _productRepository = productRepository;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }

        public async Task<Response<ProductResponseDtoForAdmin>> Handle(CreateProductAsyncCommand request, CancellationToken cancellationToken)
        {
            var ProductDto = request.ProductDto;
            List<string> uploadedImageUrls = new();
            bool transactionStarted = false;

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

                await _productRepository.BeginTransactionAsync();
                transactionStarted = true;

                var product = _mapper.Map<SmartCare.Domain.Entities.Product>(ProductDto);

                if (uploadedImageUrls.Any())
                {
                    product.Images = uploadedImageUrls
                    .Select(url => new ProductImage { Url = url })
                    .ToList();

                }

                var createdEntity = await _productRepository.AddAsync(product);

                if (createdEntity is null)
                    throw new InvalidOperationException("Product creation failed.");

                await _productRepository.SaveChangesAsync();
                await _productRepository.CommitTransactionAsync();

                await _redisCacheService.DeleteKeysByTag(tag);

                var createdProductDto = _mapper.Map<ProductResponseDtoForAdmin>(createdEntity);
                return _responseHandler.Success(createdProductDto, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                if (transactionStarted)
                    await _productRepository.RollBackAsync();

                foreach (var url in uploadedImageUrls)
                    await _imageUploaderService.DeleteImageByUrlAsync(url);


                return _responseHandler.Failed<ProductResponseDtoForAdmin>(SystemMessages.FAILED);
            }
        }
    }
}
