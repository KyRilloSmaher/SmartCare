using AutoMapper;
using SmartCare.Application.DTOs.Rates.Requests;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace SmartCare.InfraStructure.Services
{
    public class RateService : IRateService
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


        #region Constructors
        public RateService(
            IRateRepository rateRepository,
            IProductRepository productRepository,
            IClientRepository clientRepository,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            IResponseHandler responseHandler)
        {
            _rateRepository = rateRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _responseHandler = responseHandler;
        }

        #endregion

        #region Methods
        public async Task<Response<RateResponseDto>> GetRateByIdAsync(Guid Id)
        {
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

        public async Task<Response<IEnumerable<RateResponseDto>>> GetAllRatesForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"rates_user_{userId}";

            try
            {
                var cachedRates = await _redisCacheService.GetDataAsync<IEnumerable<RateResponseDto>>(cacheKey, Rate_tag);
                if (cachedRates != null) return _responseHandler.Success(cachedRates);
            }
            catch (Exception) { }

            var client = await _clientRepository.GetByIdAsync(userId);
            if (client == null)
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.USER_NOT_FOUND);

            var rates = await _rateRepository.GetRatesByUserIdAsync(userId);
            var rateDtos = _mapper.Map<IEnumerable<RateResponseDto>>(rates);

            await _redisCacheService.SetDataAsync(cacheKey, rateDtos, Rate_tag, Time.Default);
            return _responseHandler.Success(rateDtos);
        }

        public async Task<Response<IEnumerable<RateResponseDto>>> GetAllRatesForProductAsync(Guid Id)
        {
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

        public async Task<Response<RateResponseDto>> CreateRateAsync(string userId , CreateRateRequestDto Dto)
        {
            var user = await _clientRepository.GetByIdAsync(userId ,true);
            if (user == null)
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.USER_NOT_FOUND);
            }
            var product = await _productRepository.GetByIdAsync(Dto.ProductId);
            if (product == null)
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (await _rateRepository.IsProductRatedByUserAsync(userId, Dto.ProductId))
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.RATE_ALREADY_EXISTS);
            }
            var rate = _mapper.Map<Rate>(Dto);
            rate.ClientId = userId;
            var savedRate = await _rateRepository.AddAsync(rate);
            user.RatesCount++;
            await _clientRepository.UpdateAsync(user);
            await _rateRepository.UpdateAverageRateForProductAsync(Dto.ProductId);
            var rateDto = _mapper.Map<RateResponseDto>(savedRate);

            string product_detailsKey = $"product_admin_{Dto.ProductId}";
            string product_NameEnKey = $"product_name_{product.NameEn.ToLower().Replace(" ", "_")}";
            string rates_ByIdKey = $"rate_{Dto.ProductId}";
            string rates_ForUserKey = $"rates_user_{userId}";
            string rates_ForProductKey = $"rates_product_{Dto.ProductId}";

            await _redisCacheService.RemoveKeyAsync(product_detailsKey , Products_tag);
            await _redisCacheService.RemoveKeyAsync(product_NameEnKey, Products_tag);
            await _redisCacheService.RemoveKeyAsync(rates_ByIdKey, Rate_tag );
            await _redisCacheService.RemoveKeyAsync(rates_ForUserKey, Rate_tag);
            await _redisCacheService.RemoveKeyAsync(rates_ForProductKey, Rate_tag);



            return _responseHandler.Success(rateDto);
        }

        public  async Task<Response<RateResponseDto>> UpdateRateAsync(string userId, UpdateRateRequestDto Dto)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.INVALID_INPUT);
            }
            var user = await _clientRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.USER_NOT_FOUND);
            }
            var existingRate = await _rateRepository.GetByIdAsync(Dto.Id);
            if (existingRate == null)
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.RATE_NOT_FOUND);
            }
            _mapper.Map(Dto, existingRate);
            await _rateRepository.UpdateAsync(existingRate);
            await _rateRepository.UpdateAverageRateForProductAsync(Dto.ProductId);
            var rateDto = _mapper.Map<RateResponseDto>(existingRate);
            return _responseHandler.Success(rateDto);
        }

        public async Task<Response<bool>> DeleteRateAsync(string userId ,Guid Id)
        {
            if (string.IsNullOrEmpty(userId) || Id == Guid.Empty)
            {
                return _responseHandler.Failed<bool>(SystemMessages.INVALID_INPUT);
            }
            var user = await _clientRepository.GetByIdAsync(userId ,true);
            if (user == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            }
            var existingRate = await _rateRepository.GetByIdAsync(Id,true);
            if (existingRate == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.RATE_NOT_FOUND);
            }
            user.RatesCount--;
            await _clientRepository.UpdateAsync(user);
            await _rateRepository.DeleteAsync(existingRate);
            await _rateRepository.UpdateAverageRateForProductAsync(existingRate.ProductId);
            return _responseHandler.Success(true);
        }
        #endregion
    }
}
