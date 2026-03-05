using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Favourite.Queries;
using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Favourite.Handlers
{
    public class GetAllFavouritesForUserHandler : IRequestHandler<GetAllFavouritesForUserAsyncQuery, Response<IEnumerable<FavoriteResponseDto>>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly IRedisCacheService _redisCacheService;
        private readonly string tag = CacheConstants.Favourite;
        #endregion

        public GetAllFavouritesForUserHandler(
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

        public async Task<Response<IEnumerable<FavoriteResponseDto>>> Handle(GetAllFavouritesForUserAsyncQuery request, CancellationToken cancellationToken)
        {
            var userId = request.userId;

            if (string.IsNullOrEmpty(userId))
            {
                return _responseHandler.Failed<IEnumerable<FavoriteResponseDto>>(SystemMessages.INVALID_INPUT);
            }

            string cacheKey = $"fav_user_{userId.ToLower().Trim()}";

            try
            {
                var cachedFavs = await _redisCacheService.GetDataAsync<IEnumerable<FavoriteResponseDto>>(cacheKey, tag);
                if (cachedFavs != null)
                {
                    return _responseHandler.Success(cachedFavs);
                }
            }
            catch (Exception) { /* Error Redis */ }

            var client = await _unitOfWork.Clients.GetByIdAsync(userId);
            if (client == null)
            {
                return _responseHandler.Failed<IEnumerable<FavoriteResponseDto>>(SystemMessages.NOT_FOUND);
            }

            var Favourites = await _unitOfWork.Favourites.GetFavouritesByUserIdAsync(userId);
            var FavouritesDtos = _mapper.Map<IEnumerable<FavoriteResponseDto>>(Favourites);

            try
            {
                if (FavouritesDtos != null)
                {
                    await _redisCacheService.SetDataAsync(cacheKey, FavouritesDtos, tag, Time.Default);
                }
            }
            catch (Exception) { }

            return _responseHandler.Success(FavouritesDtos);
        }
    }
}