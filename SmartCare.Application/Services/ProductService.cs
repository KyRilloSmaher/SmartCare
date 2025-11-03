using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Extentions;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Services
{
    public class ProductService : IProductService
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IProductRepository _productRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor
        public ProductService(IResponseHandler responseHandler, IProductRepository productRepository, IImageUploaderService imageUploaderService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _productRepository = productRepository;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }
        #endregion

        #region Methods      
        public async Task<Response<ProductResponseDtoForClient>> GetDetailsOfProductForUser(Guid productId)
        {
            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<ProductResponseDtoForClient>(SystemMessages.INVALID_INPUT);
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return _responseHandler.Failed<ProductResponseDtoForClient>(SystemMessages.NOT_FOUND);
            var ProductDto = _mapper.Map<ProductResponseDtoForClient>(product);
            return _responseHandler.Success(ProductDto);
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetAllProducts(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.GetAllProductsQuerable();
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);

        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetProductsByCompanyId(Guid CompanyId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.GetProductsByCompanyId(CompanyId);
            if (query == null)
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> SearchProductsByCompanyName(string CompanyName, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
             var query =  _productRepository.SearchProductsByCompanyName(CompanyName);
            if(!await query.AnyAsync())
              return _responseHandler.Failed< PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult =await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetProductsByCategoryId(Guid CategoryId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.GetProductsByCategoryId(CategoryId);
            if (query == null)
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> SearchProductsByCategoryName(string CategoryName, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.SearchProductsByCategoryName(CategoryName);
            if (!await query.AnyAsync())
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetexpiredProducts(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.GetExpiredProducts();
            if (query == null)
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetUnexpiredProducts(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.GetUnExpiredProducts();
            if (query == null)
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetMostSellingProducts(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.GetMostSelling();
            if (query == null)
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }

        public async Task<Response<ProductResponseDtoForAdmin>> GetDetailsOfProductForAdmin(Guid productId)
        {
            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<ProductResponseDtoForAdmin>(SystemMessages.INVALID_INPUT);
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return _responseHandler.Failed<ProductResponseDtoForAdmin>(SystemMessages.NOT_FOUND);
            var ProductDto = _mapper.Map<ProductResponseDtoForAdmin>(product);
            return _responseHandler.Success(ProductDto);
        }

        public async Task<Response<ProductResponseDtoForClient>> GetDetailsOfProductByName(string? NameEn)
        {
            if (string.IsNullOrWhiteSpace(NameEn))
                return _responseHandler.Failed<ProductResponseDtoForClient>("Product name must be provided.");

            var product = await _productRepository.SearchProductByNameAsync(NameEn);
            if (product == null)
                return _responseHandler.Failed<ProductResponseDtoForClient>("Product not found.");

            var productDto = _mapper.Map<ProductResponseDtoForClient>(product);
            return _responseHandler.Success(productDto);

        }

        public  async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> FilterProducts(FilterProductsDTo filterproduct, string orderByField, bool isAscending, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            var query =  _productRepository.FilterProductsAsync(filterproduct);

            Expression<Func<Product, object>> orderBy = orderByField.ToLower() switch
            {
                "price" => p => p.Price,
                "nameen" => p => p.NameEn,
                "namear" => p => p.NameAr,
                "discount" => p => p.DiscountPercentage,
                "expirationdate" => p => p.ExpirationDate,
                _ => p => p.NameEn 
            };

            query = isAscending
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);

            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);

            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            return _responseHandler.Success(paginatedResult);

        }

        public async Task<Response<ProductResponseDtoForAdmin>> CreateProductAsync(CreateProductRequestDto ProductDto)
        {
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

                var product = _mapper.Map<Product>(ProductDto);

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

        public async Task<Response<ProductResponseDtoForAdmin>> UpdateProductAsync(Guid Id, UpdateProductRequestDto ProductDto)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<ProductResponseDtoForAdmin>(SystemMessages.INVALID_INPUT);
            var product = await _productRepository.GetByIdAsync(Id, true);
            if (product == null)
                return _responseHandler.NotFound<ProductResponseDtoForAdmin>(SystemMessages.NOT_FOUND);
            _mapper.Map(ProductDto, product);
            var updatedProduct = await _productRepository.UpdateAsync(product);
            var updatedProductDto = _mapper.Map<ProductResponseDtoForAdmin>(updatedProduct);
            return _responseHandler.Success(updatedProductDto, SystemMessages.RECORD_UPDATED);
        }

        public async Task<Response<bool>> DeleteProductAsync(Guid productId)
        {
            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            var result = await _productRepository.DeleteAsync(product);
            return result ? _responseHandler.Success(true, SystemMessages.RECORD_DELETED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> SearchProductsByDescription(string Description, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.SearchProductsByDescriptionAsync(Description);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetMorePopular(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.GetMorePopular();
            if (query == null)
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }

        #endregion

    }
}
