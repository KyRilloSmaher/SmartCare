using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Company.Queries.GetById
{
    internal class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, Response<CompanyResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCompanyByIdQueryHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public GetCompanyByIdQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            ILogger<GetCompanyByIdQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<CompanyResponseDto>> Handle(GetCompanyByIdQuery request,CancellationToken cancellationToken)
        {
            var id = request.Id;

            if (id == Guid.Empty)
                return _responseHandler.BadRequest<CompanyResponseDto>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"{CacheConstants.Company}_{id}";

            try
            {
                var cachedCompany = await _redisCacheService
                    .GetDataAsync<CompanyResponseDto>(cacheKey, tag);

                if (cachedCompany != null)
                    return _responseHandler.Success(cachedCompany);
            }
            catch
            {
                _logger.LogError("Error Occured Will Retrieving Company By id through Cahce");
            }

            var Company = await _unitOfWork.Companies.GetByIdAsync(id);

            if (Company == null)
                return _responseHandler.Failed<CompanyResponseDto>(SystemMessages.NOT_FOUND);

            var CompanyDto = _mapper.Map<CompanyResponseDto>(Company);

            await _redisCacheService
                .SetDataAsync(cacheKey, CompanyDto, tag, Time.Default);

            return _responseHandler.Success(CompanyDto);
        }
    }
}
