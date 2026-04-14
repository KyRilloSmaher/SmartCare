using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCare.Application.DTOs.Product.Responses;
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
    public class SearchProductsByNameInStoreHandler : IRequestHandler<SearchProductsByNameInStoreQuery, Response<IEnumerable<ProductResponseDtoForPharmacist>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public SearchProductsByNameInStoreHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ProductResponseDtoForPharmacist>>> Handle(
            SearchProductsByNameInStoreQuery request, CancellationToken cancellationToken)
        {
            var productName = request.ProductName?.Trim();
            var storeId = request.StoreId;

            if (string.IsNullOrWhiteSpace(productName))
                return _responseHandler.BadRequest<IEnumerable<ProductResponseDtoForPharmacist>>(
                    SystemMessages.PRODUCT_NOT_FOUND);

            var query = _unitOfWork.Inventories.SearchInventoriesByProductNameInStore(productName, storeId);

            if (query == null)
                return _responseHandler.Failed<IEnumerable<ProductResponseDtoForPharmacist>>(
                    SystemMessages.PRODUCT_NOT_FOUND);

            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForPharmacist>(query);
            var result = await projectedQuery.ToListAsync();

            if (!result.Any())
                return _responseHandler.NotFound<IEnumerable<ProductResponseDtoForPharmacist>>(
                    SystemMessages.NOT_FOUND);

            return _responseHandler.Success<IEnumerable<ProductResponseDtoForPharmacist>>(result);
        }
    }
}
