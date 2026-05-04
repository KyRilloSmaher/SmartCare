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
    public class GetProductsByCompanyInStoreHandler : IRequestHandler<GetProductsByCompanyInStoreQuery, Response<PaginatedResult<ProductResponseDtoForPharmacist>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public GetProductsByCompanyInStoreHandler(
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
            GetProductsByCompanyInStoreQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber;
            var pageSize = request.PageSize;
            var companyId = request.CompanyId;
            var storeId = request.StoreId;

            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForPharmacist>>(
                    SystemMessages.INVALID_PAGINATION_PARAMETERS);


            var query = _unitOfWork.Inventories.GetInventoriesByCompanyInStore(companyId, storeId);

            if (query == null)
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForPharmacist>>(
                    SystemMessages.NOT_FOUND);

            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForPharmacist>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            //if (paginatedResult != null)
            //    await _redisCacheService.SetDataAsync(cacheKey, paginatedResult, tag, Time.Default);

            return _responseHandler.Success(paginatedResult);
        }
    }
}
