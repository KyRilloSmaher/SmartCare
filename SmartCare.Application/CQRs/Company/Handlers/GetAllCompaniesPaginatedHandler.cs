using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Company.Queries;
using SmartCare.Application.DTOs.Companies.Responses;
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

namespace SmartCare.Application.CQRs.Company.Handlers
{
    public class GetAllCompaniesPaginatedHandler : IRequestHandler<GetAllCompaniesPaginatedAsyncQuery, Response<PaginatedResult<CompanyResponseDto>>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICompanyRepository _CompanyRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Companies;

        #endregion
        public GetAllCompaniesPaginatedHandler(
            IResponseHandler responseHandler,
            ICompanyRepository companyRepository,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _CompanyRepository = companyRepository;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }
        public async Task<Response<PaginatedResult<CompanyResponseDto>>> Handle(GetAllCompaniesPaginatedAsyncQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<CompanyResponseDto>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            string cacheKey = $"companies_all_p{pageNumber}_s{pageSize}";

            try
            {
                var cachedData = await _redisCacheService.GetDataAsync<PaginatedResult<CompanyResponseDto>>(cacheKey, tag);
                if (cachedData != null) return _responseHandler.Success(cachedData);
            }
            catch (Exception) { /* Fallback to DB */ }

            var query = _CompanyRepository.GetAllCompaniesQuerable();
            var projectedQuery = _mapper.ProjectTo<CompanyResponseDto>(query);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            if (paginatedResult != null)
            {
                await _redisCacheService.SetDataAsync(cacheKey, paginatedResult, tag, Time.Default);
            }

            return _responseHandler.Success(paginatedResult);
        }
    }
}
