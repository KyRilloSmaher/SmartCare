using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Rate.Commands;
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
    public class DeleteRateHandler : IRequestHandler<DeleteRateAsyncCommand, Response<bool>>
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

        public DeleteRateHandler(IRateRepository rateRepository, IProductRepository productRepository, IClientRepository clientRepository, IRedisCacheService redisCacheService, IMapper mapper, IResponseHandler responseHandler)
        {
            _rateRepository = rateRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
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
            var user = await _clientRepository.GetByIdAsync(userId, true);
            if (user == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            }
            var existingRate = await _rateRepository.GetByIdAsync(Id, true);
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
    }
}
