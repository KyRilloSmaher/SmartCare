using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class SearchProductsByCompanyNameHandler : IRequestHandler<SearchProductsByCompanyNameQuery, Response<PaginatedResult<ProductResponseDtoForClient>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public SearchProductsByCompanyNameHandler(
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

        public async Task<Response<PaginatedResult<ProductResponseDtoForClient>>> Handle(SearchProductsByCompanyNameQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;
            var CompanyName = request.CompanyName;

            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            var query = _unitOfWork.Products.SearchProductsByCompanyName(CompanyName);

            if (!await query.AnyAsync(cancellationToken))
                return _responseHandler.Failed<PaginatedResult<ProductResponseDtoForClient>>(SystemMessages.NOT_FOUND);

            var projectedQuery = _mapper.ProjectTo<ProductResponseDtoForClient>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            return _responseHandler.Success(paginatedResult);
        }
    }
}