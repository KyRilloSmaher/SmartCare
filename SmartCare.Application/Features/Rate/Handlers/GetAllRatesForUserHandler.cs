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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Rate.Handlers
{
    public class GetAllRatesForUserHandler : IRequestHandler<GetAllRatesForUserAsyncQuery, Response<IEnumerable<RateResponseDto>>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly string Rate_tag = CacheConstants.Rates;
        private readonly string Products_tag = CacheConstants.Products;
        #endregion

        public GetAllRatesForUserHandler(
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            IResponseHandler responseHandler)
        {
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _responseHandler = responseHandler;
        }

        public async Task<Response<IEnumerable<RateResponseDto>>> Handle(GetAllRatesForUserAsyncQuery request, CancellationToken cancellationToken)
        {
            var userId = request.userId;

            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"rates_user_{userId}";

            try
            {
                var cachedRates = await _redisCacheService.GetDataAsync<IEnumerable<RateResponseDto>>(cacheKey, Rate_tag);
                if (cachedRates != null)
                    return _responseHandler.Success(cachedRates);
            }
            catch (Exception) { }

            var client = await _unitOfWork.Clients.GetByIdAsync(userId);
            if (client == null)
                return _responseHandler.Failed<IEnumerable<RateResponseDto>>(SystemMessages.USER_NOT_FOUND);

            var rates = await _unitOfWork.Rates.GetRatesByUserIdAsync(userId);
            var rateDtos = _mapper.Map<IEnumerable<RateResponseDto>>(rates);

            await _redisCacheService.SetDataAsync(cacheKey, rateDtos, Rate_tag, Time.Default);
            return _responseHandler.Success(rateDtos);
        }
    }
}