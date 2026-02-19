using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Rate.Queries;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Rate.Handlers
{
    public class GetRateByIdHandler : IRequestHandler<GetRateByIdAsyncQuery, Response<RateResponseDto>>
    {
        #region Feilds
        private readonly IRateRepository _rateRepository;
        private readonly IProductRepository _productRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        string Rate_tag = CacheConstants.Rates;
        string Products_tag = CacheConstants.Products;

        #endregion

        public GetRateByIdHandler(IRateRepository rateRepository, IProductRepository productRepository, IClientRepository clientRepository, IRedisCacheService redisCacheService, IMapper mapper, IResponseHandler responseHandler)
        {
            _rateRepository = rateRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _responseHandler = responseHandler;
        }

        public async Task<Response<RateResponseDto>> Handle(GetRateByIdAsyncQuery request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            if (Id == Guid.Empty)
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"rate_{Id}";

            try
            {
                var cachedRate = await _redisCacheService.GetDataAsync<RateResponseDto>(cacheKey, Rate_tag);
                if (cachedRate != null) return _responseHandler.Success(cachedRate);
            }
            catch (Exception) { }

            var rate = await _rateRepository.GetByIdAsync(Id);
            if (rate == null)
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.NOT_FOUND);

            var rateDto = _mapper.Map<RateResponseDto>(rate);

            await _redisCacheService.SetDataAsync(cacheKey, rateDto, Rate_tag, Time.Default);
            return _responseHandler.Success(rateDto);
        }
    }
}
