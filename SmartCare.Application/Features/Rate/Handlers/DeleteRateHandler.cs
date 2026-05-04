using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Rate.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Rate.Handlers
{
    public class DeleteRateHandler : IRequestHandler<DeleteRateAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly string Rate_tag = CacheConstants.Rates;
        private readonly string Products_tag = CacheConstants.Products;
        #endregion

        public DeleteRateHandler(
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

        public async Task<Response<bool>> Handle(DeleteRateAsyncCommand request, CancellationToken cancellationToken)
        {
            var userId = request.userId;
            var Id = request.Id;

            if (string.IsNullOrEmpty(userId) || Id == Guid.Empty)
            {
                return _responseHandler.Failed<bool>(SystemMessages.INVALID_INPUT);
            }

            var user = await _unitOfWork.Clients.GetByIdAsync(userId, true);
            if (user == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            }

            var existingRate = await _unitOfWork.Rates.GetByIdAsync(Id, true);
            if (existingRate == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.RATE_NOT_FOUND);
            }

            var productId = existingRate.ProductId;

            user.RatesCount--;
            await _unitOfWork.Rates.DeleteAsync(existingRate);

            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update average rate for product (separate operation)
            await _unitOfWork.Rates.UpdateAverageRateForProductAsync(productId);

            // Clear cache keys
            string rates_ForUserKey = $"rates_user_{userId}";
            string rates_ForProductKey = $"rates_product_{productId}";
            string clientByIdKey = $"client_id_{userId}";
            string clientByEmailKey = $"client_email_{user.User?.Email?.ToLower()}";

            await _redisCacheService.RemoveKeyAsync(rates_ForUserKey, Rate_tag);
            await _redisCacheService.RemoveKeyAsync(rates_ForProductKey, Rate_tag);
            await _redisCacheService.RemoveKeyAsync(clientByIdKey, CacheConstants.Client);
            await _redisCacheService.RemoveKeyAsync(clientByEmailKey, CacheConstants.Client);
            await _redisCacheService.RemoveKeyAsync("clients_all", CacheConstants.Client);

            return _responseHandler.Success(true);
        }
    }
}