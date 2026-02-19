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
    public class GetAllRatesForProductHandler : IRequestHandler<GetAllRatesForProductAsyncQuery, Response<IEnumerable<RateResponseDto>>>
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

        public GetAllRatesForProductHandler(IRateRepository rateRepository, IProductRepository productRepository, IClientRepository clientRepository, IRedisCacheService redisCacheService, IMapper mapper, IResponseHandler responseHandler)
        {
            _rateRepository = rateRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _responseHandler = responseHandler;
        }
        public async Task<Response<IEnumerable<RateResponseDto>>> Handle(GetAllRatesForProductAsyncQuery request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            if (Id == Guid.Empty)
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"rates_product_{Id}";

            try
            {
                var cachedRates = await _redisCacheService.GetDataAsync<IEnumerable<RateResponseDto>>(cacheKey, Rate_tag);
                if (cachedRates != null) return _responseHandler.Success(cachedRates);
            }
            catch (Exception) { }

            var product = await _productRepository.GetByIdAsync(Id);
            if (product == null)
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.PRODUCT_NOT_FOUND);

            var rates = await _rateRepository.GetRatesByProductIdAsync(Id);
            var rateDtos = _mapper.Map<IEnumerable<RateResponseDto>>(rates);

            await _redisCacheService.SetDataAsync(cacheKey, rateDtos, Rate_tag, Time.Default);
            return _responseHandler.Success(rateDtos);
        }
    }
}
