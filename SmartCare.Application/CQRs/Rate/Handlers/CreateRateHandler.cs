using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Rate.Commands;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Rate.Handlers
{
    public class CreateRateHandler : IRequestHandler<CreateRateAsyncCommand, Response<RateResponseDto>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly string Rate_tag = CacheConstants.Rates;
        private readonly string Products_tag = CacheConstants.Products;
        #endregion

        public CreateRateHandler(
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

        public async Task<Response<RateResponseDto>> Handle(CreateRateAsyncCommand request, CancellationToken cancellationToken)
        {
            var Dto = request.Dto;
            var userId = request.userId;

            var user = await _unitOfWork.Clients.GetByIdAsync(userId, true);
            if (user == null)
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.USER_NOT_FOUND);
            }

            var product = await _unitOfWork.Products.GetByIdAsync(Dto.ProductId);
            if (product == null)
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }

            if (await _unitOfWork.Rates.IsProductRatedByUserAsync(userId, Dto.ProductId))
            {
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.RATE_ALREADY_EXISTS);
            }

            var rate = _mapper.Map<SmartCare.Domain.Entities.Rate>(Dto);
            rate.ClientId = userId;

            var savedRate = await _unitOfWork.Rates.AddAsync(rate);
            user.RatesCount++;

            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update average rate for product (this might be a separate operation that doesn't need to be in the same transaction)
            await _unitOfWork.Rates.UpdateAverageRateForProductAsync(Dto.ProductId);

            var rateDto = _mapper.Map<RateResponseDto>(savedRate);

            // Clear cache keys
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