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
namespace SmartCare.Application.Features.Product.Commands.Create
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Response<ProductResponseDtoForAdmin>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public CreateProductCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IImageUploaderService imageUploaderService,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            IBackgroundJobService backgroundJobService)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _backgroundJobService = backgroundJobService;
        }

        public async Task<Response<ProductResponseDtoForAdmin>> Handle(CreateProductCommand request,CancellationToken cancellationToken)
        {
            var dto = request.ProductDto;

            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            var company = await _unitOfWork.Companies.GetByIdAsync(dto.CompanyId);

            if (category is null || company is null)
                return _responseHandler.BadRequest<ProductResponseDtoForAdmin>("Invalid category or company");

            var product = _mapper.Map<SmartCare.Domain.Entities.Product>(dto);
            product.ProductId = Guid.NewGuid();

            var images = new List<ProductImage>();

            try
            {
                //  Upload Main Image
                var mainUpload = await _imageUploaderService
                    .UploadImageAsync(dto.MainImage, ImageFolder.ProductImages);

                if (mainUpload.Error != null)
                    return _responseHandler.Failed<ProductResponseDtoForAdmin>("Main image upload failed");

                images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    Url = mainUpload.Url.ToString(),
                    IsPrimary = true,
                    ProductId = product.ProductId
                });

                //  Upload Additional Images
                if (dto.Images != null && dto.Images.Any())
                {
                    var uploads = await _imageUploaderService
                        .UploadMultipleImagesAsync(dto.Images, ImageFolder.ProductImages);

                    if (uploads.Any(x => x.Error != null))
                        return _responseHandler.Failed<ProductResponseDtoForAdmin>("Image upload failed");

                    foreach (var upload in uploads)
                    {
                        images.Add(new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            Url = upload.Url.ToString(),
                            IsPrimary = false,
                            ProductId = product.ProductId
                        });
                    }
                }

                product.Images = images;

                var created = await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _redisCacheService.DeleteKeysByTag(tag);
                var responseDto = _mapper.Map<ProductResponseDtoForAdmin>(created);
                _backgroundJobService.Enqueue(() => _unitOfWork.Inventories.CreateInventoriesForProduct(product.ProductId));
                return _responseHandler.Success(responseDto, SystemMessages.SUCCESS);
            }
            catch
            {
                foreach (var img in images)
                    await _imageUploaderService.DeleteImageByUrlAsync(img.Url);

                return _responseHandler.Failed<ProductResponseDtoForAdmin>(SystemMessages.FAILED);
            }
        }
    }
}