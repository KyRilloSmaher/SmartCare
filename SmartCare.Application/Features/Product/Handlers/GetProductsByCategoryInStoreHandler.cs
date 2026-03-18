using AutoMapper;
using MediatR;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Extentions;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Product.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Handlers
{
    public class GetProductsByCategoryInStoreHandler : IRequestHandler<GetProductsByCategoryInStoreQuery, Response<PaginatedResult<ProductResponseDtoForPharmacist>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public GetProductsByCategoryInStoreHandler(
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

        public async Task<Response<PaginatedResult<ProductResponseDtoForPharmacist>>> Handle(
            GetProductsByCategoryInStoreQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber;
            var pageSize = request.PageSize;
            var categoryId = request.CategoryId;
            var storeId = request.StoreId;

            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForPharmacist>>(
                    SystemMessages.INVALID_PAGINATION_PARAMETERS);

            string cacheKey = $"products_category_{categoryId}_store_{storeId}_p{pageNumber}_s{pageSize}";

            try
            {
                var cachedData = await _redisCacheService.GetDataAsync<PaginatedResult<ProductResponseDtoForPharmacist>>(cacheKey, tag);
                if (cachedData != null)
                    return _responseHandler.Success(cachedData);
            }
            catch (Exception)
            {
                // Ignore cache errors
            }

            var query = _unitOfWork.Inventories.GetInventoriesByCategoryInStore(categoryId, storeId);

            if (query == null)
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForPharmacist>>(
                    SystemMessages.NOT_FOUND);

            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForPharmacist>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            if (paginatedResult != null)
                await _redisCacheService.SetDataAsync(cacheKey, paginatedResult, tag, Time.Default);

            return _responseHandler.Success(paginatedResult);
        }
    }
}
