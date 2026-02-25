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
    public class UpdateRateHandler : IRequestHandler<UpdateRateAsyncCommand, Response<RateResponseDto>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly string Rate_tag = CacheConstants.Rates;
        private readonly string Products_tag = CacheConstants.Products;
        #endregion

        public UpdateRateHandler(
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

        public async Task<Response<RateResponseDto>> Handle(UpdateRateAsyncCommand request, CancellationToken cancellationToken)
        {
            
            var Dto = request.Dto;
            var Id = Dto.Id;
            var userId = request.userId;

            if (Id == Guid.Empty || Dto == null || string.IsNullOrEmpty(userId))
                return _responseHandler.BadRequest<RateResponseDto>(SystemMessages.INVALID_INPUT);

            // Verify user exists
            var user = await _unitOfWork.Clients.GetByIdAsync(userId);
            if (user == null)
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.USER_NOT_FOUND);

            // Get existing rate
            var existingRate = await _unitOfWork.Rates.GetByIdAsync(Id, true);
            if (existingRate == null)
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.RATE_NOT_FOUND);

            // Verify ownership
            if (existingRate.ClientId != userId)
                return _responseHandler.Failed<RateResponseDto>(SystemMessages.UNAUTHORIZED);

            // Store product ID for cache clearing and average update
            var productId = existingRate.ProductId;
            var oldRatingValue = existingRate.Value;

            // Update rate
            _mapper.Map(Dto, existingRate);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update average rate if rating value changed
            if (oldRatingValue != existingRate.Value)
            {
                await _unitOfWork.Rates.UpdateAverageRateForProductAsync(productId);
            }

            // Clear cache keys
            string rateKey = $"rate_{Id}";
            string ratesForUserKey = $"rates_user_{userId}";
            string ratesForProductKey = $"rates_product_{productId}";

            await _redisCacheService.RemoveKeyAsync(rateKey, Rate_tag);
            await _redisCacheService.RemoveKeyAsync(ratesForUserKey, Rate_tag);
            await _redisCacheService.RemoveKeyAsync(ratesForProductKey, Rate_tag);

            var rateDto = _mapper.Map<RateResponseDto>(existingRate);
            return _responseHandler.Success(rateDto, SystemMessages.RECORD_UPDATED);
        }
    }
}