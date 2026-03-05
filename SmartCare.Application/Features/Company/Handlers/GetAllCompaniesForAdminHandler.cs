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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Company.Handlers
{
    public class GetAllCompaniesForAdminHandler : IRequestHandler<GetAllCompaniesForAdminAsyncQuery, Response<IEnumerable<CompanyResponseForAdminDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public GetAllCompaniesForAdminHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
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
                if (cached != null)
                    return _responseHandler.Success(cached);
            }
            catch (Exception) { }

            var companies = await _unitOfWork.Companies.GetAllCompaniesForAdminAsync();
            var companiesDto = _mapper.Map<IEnumerable<CompanyResponseForAdminDto>>(companies);

            await _redisCacheService.SetDataAsync(cacheKey, companiesDto, tag, Time.Default);

            return _responseHandler.Success(companiesDto);
        }
    }
}