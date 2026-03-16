using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Extentions;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Company.Queries.GetAllByPaginated
{
    public class GetAllCompaniesPaginatedQueryHandler: IRequestHandler<GetAllCompaniesPaginatedQuery, Response<PaginatedResult<CompanyResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllCompaniesPaginatedQueryHandler> _logger;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public GetAllCompaniesPaginatedQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            ILogger<GetAllCompaniesPaginatedQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<PaginatedResult<CompanyResponseDto>>> Handle(GetAllCompaniesPaginatedQuery request,CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;

            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler
                    .BadRequest<PaginatedResult<CompanyResponseDto>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            string cacheKey = $"{CacheConstants.Company}_all_p{pageNumber}_s{pageSize}";

                
                try
                {
                    var cached = await _redisCacheService
                        .GetDataAsync<PaginatedResult<CompanyResponseDto>>(cacheKey, tag);

                    if (cached != null)
                    {
                        _logger.LogInformation("Companies page {Page} returned from cache", pageNumber);
                        return _responseHandler.Success(cached);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve paginated companies from cache");
                }
                var query = _unitOfWork.Companies.GetAllCompaniesQuerable();

                var projectedQuery = _mapper
                    .ProjectTo<CompanyResponseDto>(query);

                var paginatedResult = await projectedQuery
                    .ToPaginatedListAsync(pageNumber, pageSize);

                // Cache result
                try
                {
                    await _redisCacheService
                        .SetDataAsync(cacheKey, paginatedResult, tag, Time.Default);

                    _logger.LogInformation("Cached companies page {Page}", pageNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cache companies page {Page}", pageNumber);
                }

                return _responseHandler.Success(paginatedResult);
           
        }
    }
}