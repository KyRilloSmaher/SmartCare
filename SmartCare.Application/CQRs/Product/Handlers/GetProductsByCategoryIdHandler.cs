using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Product.Queries;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Extentions;
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

namespace SmartCare.Application.CQRs.Product.Handlers
{
    public class GetProductsByCategoryIdHandler : IRequestHandler<GetProductsByCategoryIdQuery, Response<PaginatedResult<ProductResponseDtoForClient>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IProductRepository _productRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Products;


        #endregion

        public GetProductsByCategoryIdHandler(IResponseHandler responseHandler, IProductRepository productRepository, IImageUploaderService imageUploaderService, IRedisCacheService redisCacheService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _productRepository = productRepository;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> Handle(GetProductsByCategoryIdQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;
            var CategoryId = request.CategoryId;
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            string cacheKey = $"products_category_{CategoryId}_p{pageNumber}_s{pageSize}";


            try
            {
                var cachedData = await _redisCacheService.GetDataAsync<PaginatedResult<ProductResponseDtoForClient>>(cacheKey, tag);

                if (cachedData != null)
                {
                    return _responseHandler.Success(cachedData);
                }
            }
            catch (Exception)
            {

            }

            var query = _productRepository.GetProductsByCategoryId(CategoryId);
            if (query == null)
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);

            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            if (paginatedResult != null)
            {
                await _redisCacheService.SetDataAsync(cacheKey, paginatedResult, tag, Time.Default);
            }

            return _responseHandler.Success(paginatedResult);
        }
    }
}
