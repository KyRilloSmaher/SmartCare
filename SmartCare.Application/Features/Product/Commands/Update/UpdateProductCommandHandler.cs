using AutoMapper;
using MediatR;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Commands.Update
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Response<ProductResponseDtoForAdmin>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public UpdateProductCommandHandler(
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

        public async Task<Response<ProductResponseDtoForAdmin>> Handle(UpdateProductCommand request,CancellationToken cancellationToken)
        {
            var dto = request.ProductDto;

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, true);

            if (product == null)
                return _responseHandler.NotFound<ProductResponseDtoForAdmin>("Product not found");

            //Track uploaded images for rollback
            var uploadedImages = new List<string>();

            try
            {
                // Validate Category
                if (dto.CategoryId.HasValue && dto.CategoryId != product.CategoryId)
                {
                    var oldCategory = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId, true);
                    var newCategory = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId.Value);

                    if (newCategory == null)
                        return _responseHandler.BadRequest<ProductResponseDtoForAdmin>("Invalid category");

                    oldCategory.decreaseProductCount();
                    newCategory.IncreaseProductCount();
                }

                //  Validate Company
                if (dto.CompanyId.HasValue && dto.CompanyId != product.CompanyId)
                {
                    var oldCompany = await _unitOfWork.Companies.GetByIdAsync(product.CompanyId, true);
                    var newCompany = await _unitOfWork.Companies.GetByIdAsync(dto.CompanyId.Value);

                    if (newCompany == null)
                        return _responseHandler.BadRequest<ProductResponseDtoForAdmin>("Invalid company");

                    oldCompany.decreaseProductCount();
                    newCompany.IncreaseProductCount();
                }

                //Map basic fields
                _mapper.Map(dto, product);
                product.UpdatedAt = DateTime.UtcNow;

                // =========================
                // Upload New Main Image 
                // =========================
                string? newMainImageUrl = null;

                if (dto.NewMainImage != null)
                {
                    var upload = await _imageUploaderService
                        .UploadImageAsync(dto.NewMainImage, ImageFolder.ProductImages);

                    if (upload.Error != null)
                        return _responseHandler.Failed<ProductResponseDtoForAdmin>("Main image upload failed");

                    newMainImageUrl = upload.Url.ToString();
                    uploadedImages.Add(newMainImageUrl);
                }

                // =========================
                // Upload Additional Images
                // =========================
                var newImages = new List<ProductImage>();

                if (dto.NewImages != null && dto.NewImages.Any())
                {
                    var uploads = await _imageUploaderService
                        .UploadMultipleImagesAsync(dto.NewImages, ImageFolder.ProductImages);

                    if (uploads.Any(x => x.Error != null))
                        throw new Exception("Image upload failed");

                    foreach (var upload in uploads)
                    {
                        var url = upload.Url.ToString();
                        uploadedImages.Add(url);

                        newImages.Add(new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            Url = url,
                            IsPrimary = false,
                            ProductId = product.ProductId
                        });
                    }
                }

                // =========================
                //  Remove Images
                // =========================
                if (dto.RemoveImageIds != null && dto.RemoveImageIds.Any())
                {
                    var imagesToRemove = product.Images
                        .Where(i => dto.RemoveImageIds.Contains(i.Id))
                        .ToList();

                    foreach (var img in imagesToRemove)
                    {
                        await _imageUploaderService.DeleteImageByUrlAsync(img.Url);
                        product.Images.Remove(img);
                    }
                }

                // =========================
                // Replace Main Image 
                // =========================
                if (newMainImageUrl != null)
                {
                    var oldMain = product.Images.FirstOrDefault(i => i.IsPrimary);

                    if (oldMain != null)
                    {
                        await _imageUploaderService.DeleteImageByUrlAsync(oldMain.Url);
                        product.Images.Remove(oldMain);
                    }

                    product.Images.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        Url = newMainImageUrl,
                        IsPrimary = true,
                        ProductId = product.ProductId
                    });
                }

                // =========================
                // Add New Images
                // =========================
                foreach (var img in newImages)
                    product.Images.Add(img);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _redisCacheService.DeleteKeysByTag(tag);

                var response = _mapper.Map<ProductResponseDtoForAdmin>(product);

                return _responseHandler.Success(response, "Updated successfully");
            }
            catch
            {
                //ROLLBACK uploaded images
                foreach (var url in uploadedImages)
                {
                    await _imageUploaderService.DeleteImageByUrlAsync(url);
                }

                return _responseHandler.Failed<ProductResponseDtoForAdmin>("Update failed");
            }
        }
    }
}