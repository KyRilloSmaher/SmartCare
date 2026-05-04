using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Favourite.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Favourite.Handlers
{
    public class DeleteFavouriteHandler : IRequestHandler<DeleteFavouriteAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly IRedisCacheService _redisCacheService;
        private readonly string tag = CacheConstants.Favourite;
        #endregion

        public DeleteFavouriteHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IResponseHandler responseHandler,
            IRedisCacheService redisCacheService)
        {
            _unitOfWork = unitOfWork;
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

            var client = await _unitOfWork.Clients.GetByIdAsync(userId);
            if (client == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
            }

            var FavouriteExist = await _unitOfWork.Favourites.CheackFavouriteExistsAsync(userId, ProductId);
            if (FavouriteExist == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
            }

            client.FavoritesCount--;
            await _unitOfWork.Favourites.DeleteAsync(FavouriteExist);

            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string cacheKey = $"fav_user_{userId}";
            string clientByIdKey = $"client_id_{userId}";
            string clientByEmailKey = $"client_email_{client.User?.Email?.ToLower()}";

            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);
            await _redisCacheService.RemoveKeyAsync(clientByIdKey, CacheConstants.Client);
            await _redisCacheService.RemoveKeyAsync(clientByEmailKey, CacheConstants.Client);
            await _redisCacheService.RemoveKeyAsync("clients_all", CacheConstants.Client);

            return _responseHandler.Success(true);
        }
    }
}