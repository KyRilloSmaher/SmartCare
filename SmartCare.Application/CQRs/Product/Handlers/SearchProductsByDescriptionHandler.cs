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
    public class SearchProductsByDescriptionHandler : IRequestHandler<SearchProductsByDescriptionQuery, Response<PaginatedResult<ProductResponseDtoForClient>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IProductRepository _productRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Products;


        #endregion

        public SearchProductsByDescriptionHandler(IResponseHandler responseHandler, IProductRepository productRepository, IImageUploaderService imageUploaderService, IRedisCacheService redisCacheService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _productRepository = productRepository;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> Handle(SearchProductsByDescriptionQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;
            var Description = request.Description;
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = _productRepository.SearchProductsByDescriptionAsync(Description);
            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }
    }
}
