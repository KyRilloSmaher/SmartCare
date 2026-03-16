using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Company.Queries.GetAllForAdmin
{
    public class GetAllCompaniesForAdminQueryHandler: IRequestHandler<GetAllCompaniesForAdminQuery, Response<IEnumerable<CompanyResponseForAdminDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllCompaniesForAdminQueryHandler> _logger;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public GetAllCompaniesForAdminQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            ILogger<GetAllCompaniesForAdminQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<IEnumerable<CompanyResponseForAdminDto>>> Handle(GetAllCompaniesForAdminQuery request,CancellationToken cancellationToken)
        {
            string cacheKey = CacheConstants.CompanyiesAllAdmin;

                try
                {
                    var cached = await _redisCacheService
                        .GetDataAsync<IEnumerable<CompanyResponseForAdminDto>>(cacheKey, tag);

                    if (cached != null)
                    {
                        _logger.LogInformation("Admin companies list returned from cache");
                        return _responseHandler.Success(cached);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve admin companies from cache");
                }

                var companies = await _unitOfWork.Companies.GetAllCompaniesForAdminAsync();
                var companiesDto = _mapper.Map<IEnumerable<CompanyResponseForAdminDto>>(companies);

                // Cache result
                try
                {
                    await _redisCacheService.SetDataAsync(cacheKey, companiesDto, tag, Time.Default);
                    _logger.LogInformation("Admin companies list cached successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cache admin companies list");
                }

                return _responseHandler.Success(companiesDto);
            
        }
    }
}