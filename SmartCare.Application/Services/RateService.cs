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

namespace SmartCare.Application.Services
{
    public class RateService : IRateService
    {
        #region Feilds
        private readonly IRateRepository _rateRepository;
        private readonly IProductRepository _productRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;

        #endregion


        #region Constructors
        public RateService(IRateRepository rateRepository, IMapper mapper, IResponseHandler responseHandler, IClientRepository clientRepository, IProductRepository productRepository)
        {
            _rateRepository = rateRepository;
            _mapper = mapper;
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _productRepository = productRepository;
        }
        #endregion 

        #region Methods
        public async Task<Response<RateResponseDto>> GetRateByIdAsync(Guid Id)
        {
            if (Id == Guid.Empty)
            {
                return  _responseHandler.Failed<RateResponseDto>(SystemMessages.INVALID_INPUT);
            }
            var rate = await _rateRepository.GetByIdAsync(Id);
            if (rate == null)
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.NOT_FOUND);
            }
            var rateDto = _mapper.Map<RateResponseDto>(rate);
            return _responseHandler.Success(rateDto);
        }

        public async Task<Response<IEnumerable<RateResponseDto>>> GetAllRatesForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.INVALID_INPUT);
            }
            var client = await _clientRepository.GetByIdAsync(userId);
            if (client == null)
            {
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.USER_NOT_FOUND);
            }
            var rates = await _rateRepository.GetRatesByUserIdAsync(userId);
            var rateDtos = _mapper.Map<IEnumerable<RateResponseDto>>(rates);
            return _responseHandler.Success(rateDtos);
        }

        public async Task<Response<IEnumerable<RateResponseDto>>> GetAllRatesForProductAsync(Guid Id)
        {
            if (Id == Guid.Empty)
            {
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.INVALID_INPUT);
            }
            var product = await _productRepository.GetByIdAsync(Id);
            if (product == null)
            {
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            var rates = await _rateRepository.GetRatesByProductIdAsync(Id);
            var rateDtos = _mapper.Map<IEnumerable<RateResponseDto>>(rates);
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
