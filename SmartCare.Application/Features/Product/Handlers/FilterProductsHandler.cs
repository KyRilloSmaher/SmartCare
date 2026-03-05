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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Product.Handlers
{
    public class FilterProductsHandler : IRequestHandler<FilterProductsQuery, Response<PaginatedResult<ProductResponseDtoForClient>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public FilterProductsHandler(
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

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> Handle(FilterProductsQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;
            var filterproduct = request.filterproduct;

            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            var query = _unitOfWork.Products.FilterProductsAsync(filterproduct);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            return _responseHandler.Success(paginatedResult);
        }
    }
}