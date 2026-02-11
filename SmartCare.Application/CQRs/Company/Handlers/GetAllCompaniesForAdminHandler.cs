using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Company.Queries;
using SmartCare.Application.DTOs.Companies.Responses;
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
    public class GetAllCompaniesForAdminHandler : IRequestHandler<GetAllCompaniesForAdminAsyncQuery, Response<IEnumerable<CompanyResponseForAdminDto>>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICompanyRepository _CompanyRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Companies;

        #endregion
        public GetAllCompaniesForAdminHandler(
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


        public async Task<Response<IEnumerable<CompanyResponseForAdminDto>>> Handle(GetAllCompaniesForAdminAsyncQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "companies_list_admin";

            try
            {
                var cached = await _redisCacheService.GetDataAsync<IEnumerable<CompanyResponseForAdminDto>>(cacheKey, tag);
                if (cached != null) return _responseHandler.Success(cached);
            }
            catch (Exception) { }

            var companies = await _CompanyRepository.GetAllCompaniesForAdminAsync();
            var companiesDto = _mapper.Map<IEnumerable<CompanyResponseForAdminDto>>(companies);

            await _redisCacheService.SetDataAsync(cacheKey, companiesDto, tag, Time.Default);

            return _responseHandler.Success(companiesDto);
        }
    }
}
