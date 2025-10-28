using AutoMapper;
using SmartCare.Application.DTOs.Favorites.Requests;
using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Services
{
    public class FavouriteService : IFavouriteService
    {
        #region Fields
        private readonly IFavouriteRepository _favouriteRepository;
        private readonly IProductRepository _productRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;


        #endregion

        #region  Constructors
        public FavouriteService(IFavouriteRepository favouriteRepository, IProductRepository productRepository, IClientRepository clientRepository, IMapper mapper, IResponseHandler responseHandler)
        {
            _favouriteRepository = favouriteRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
            _mapper = mapper;
            _responseHandler = responseHandler;
        }
        #endregion

        #region Methods
        public async Task<Response<bool>> CreateFavouriteAsync(CreateFavouriteRequestDto Dto)
        {
            var user = await _clientRepository.GetByIdAsync(Dto.ClientId , true);
            if (user == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            }
            var product = await _productRepository.GetByIdAsync(Dto.ProductId);
            if (product == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (await _favouriteRepository.IsProductFavoritedByUserAsync(Dto.ClientId, Dto.ProductId))
            {
                return _responseHandler.Failed<bool>(SystemMessages.FAVOURITE_ALREADY_EXISTS);
            }
            var Favourite = _mapper.Map<Favorite>(Dto);
            var savedFavourite = await _favouriteRepository.AddAsync(Favourite);
            user.FavoritesCount++;
            await _clientRepository.UpdateAsync(user);
            return _responseHandler.Created<bool>(true);
        }

        public async Task<Response<bool>> DeleteFavouriteAsync(string userId, Guid ProductId)
        {
            if (string.IsNullOrEmpty(userId) || ProductId == Guid.Empty)
            {
              return  _responseHandler.Failed<bool>(SystemMessages.INVALID_INPUT);
            }
            var client = await _clientRepository.GetByIdAsync(userId);
            if(client == null)
            {
               return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
            }
            var FavouriteExist = await _favouriteRepository.checkFavoriteExists(userId , ProductId);
            if(FavouriteExist == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
            }
            client.FavoritesCount--;
            await _favouriteRepository.DeleteAsync(FavouriteExist);
            return _responseHandler.Success(true);

        }

        public async Task<Response<IEnumerable<FavoriteResponseDto>>> GetAllFavouritesForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return _responseHandler.Failed<IEnumerable<FavoriteResponseDto>>(SystemMessages.INVALID_INPUT);
            }
            var client = await _clientRepository.GetByIdAsync(userId);
            if (client == null)
            {
                return _responseHandler.Failed<IEnumerable<FavoriteResponseDto>>(SystemMessages.NOT_FOUND);
            }
            var Favourites = await _favouriteRepository.GetFavouritesByUserIdAsync(userId);
            var FavouritesDtos = _mapper.Map<IEnumerable<FavoriteResponseDto>>(Favourites);
            return _responseHandler.Success(FavouritesDtos);
        }
        #endregion
    }
}
