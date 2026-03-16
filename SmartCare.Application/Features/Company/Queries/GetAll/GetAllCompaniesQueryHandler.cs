using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Company.Queries.GetAll
{
    public class GetAllCompaniesQueryHandler
        : IRequestHandler<GetAllCompaniesQuery, Response<IEnumerable<CompanyResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllCompaniesQueryHandler> _logger;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public GetAllCompaniesQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            ILogger<GetAllCompaniesQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<IEnumerable<CompanyResponseDto>>> Handle(GetAllCompaniesQuery request,CancellationToken cancellationToken)
        {
            string cacheKey = CacheConstants.CompaniesClient;

                
                try
                {
                    var cached = await _redisCacheService
                        .GetDataAsync<IEnumerable<CompanyResponseDto>>(cacheKey, tag);

                    if (cached != null)
                    {
                        _logger.LogInformation("Companies list returned from cache");
                        return _responseHandler.Success(cached);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve companies from cache");
                }


                var companies = await _unitOfWork.Companies.GetAllCompaniesAsync();

                var companiesDto = _mapper.Map<IEnumerable<CompanyResponseDto>>(companies);

                // Cache result
                try
                {
                    await _redisCacheService.SetDataAsync(cacheKey, companiesDto, tag, Time.Default);
                    _logger.LogInformation("Companies list cached successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cache companies list");
                }

                return _responseHandler.Success(companiesDto);
            
            
        }
    }
}