using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Rate.Commands;
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

    public class CreateRateHandler : IRequestHandler<CreateRateAsyncCommand, Response<RateResponseDto>>
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
        public CreateRateHandler(IRateRepository rateRepository, IProductRepository productRepository, IClientRepository clientRepository, IRedisCacheService redisCacheService, IMapper mapper, IResponseHandler responseHandler)
        {
            _rateRepository = rateRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _responseHandler = responseHandler;
        }

        public async Task<Response<RateResponseDto>> Handle(CreateRateAsyncCommand request, CancellationToken cancellationToken)
        {
            var Dto = request.Dto;
            var userId = request.userId;
            var user = await _clientRepository.GetByIdAsync(userId, true);
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
            var rate = _mapper.Map<SmartCare.Domain.Entities.Rate>(Dto);
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

            await _redisCacheService.RemoveKeyAsync(product_detailsKey, Products_tag);
            await _redisCacheService.RemoveKeyAsync(product_NameEnKey, Products_tag);
            await _redisCacheService.RemoveKeyAsync(rates_ByIdKey, Rate_tag);
            await _redisCacheService.RemoveKeyAsync(rates_ForUserKey, Rate_tag);
            await _redisCacheService.RemoveKeyAsync(rates_ForProductKey, Rate_tag);



            return _responseHandler.Success(rateDto);
        }
    }
}
