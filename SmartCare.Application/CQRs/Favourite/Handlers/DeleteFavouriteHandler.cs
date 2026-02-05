using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Favourite.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Favourite.Handlers
{
    public class DeleteFavouriteHandler : IRequestHandler<DeleteFavouriteAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IFavouriteRepository _favouriteRepository;
        private readonly IProductRepository _productRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly IRedisCacheService _redisCacheService;
        string tag = CacheConstants.Favourite;

        #endregion
        public DeleteFavouriteHandler(
            IFavouriteRepository favouriteRepository,
            IProductRepository productRepository,
            IClientRepository clientRepository,
            IMapper mapper,
            IResponseHandler responseHandler,
            IRedisCacheService redisCacheService)
        {
            _favouriteRepository = favouriteRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
            _mapper = mapper;
            _responseHandler = responseHandler;
            _redisCacheService = redisCacheService;
        }
        public async Task<Response<bool>> Handle(DeleteFavouriteAsyncCommand request, CancellationToken cancellationToken)
        {
            var userId = request.userId;
            var ProductId = request.Id;
            if (string.IsNullOrEmpty(userId) || ProductId == Guid.Empty)
            {
                return _responseHandler.Failed<bool>(SystemMessages.INVALID_INPUT);
            }
            var client = await _clientRepository.GetByIdAsync(userId);
            if (client == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
            }
            var FavouriteExist = await _favouriteRepository.checkFavoriteExists(userId, ProductId);
            if (FavouriteExist == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
            }
            client.FavoritesCount--;
            await _favouriteRepository.DeleteAsync(FavouriteExist);

            string cacheKey = $"fav_user_{userId}";

            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);

            return _responseHandler.Success(true);
        }
    }
}
