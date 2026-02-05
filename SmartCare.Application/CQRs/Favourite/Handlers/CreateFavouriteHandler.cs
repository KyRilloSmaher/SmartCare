using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Favourite.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Favourite.Handlers
{
    public class CreateFavouriteHandler : IRequestHandler<CreateFavouriteAsyncCommand, Response<bool>>
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

        public CreateFavouriteHandler(
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
        public async Task<Response<bool>> Handle(CreateFavouriteAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var user = await _clientRepository.GetByIdAsync(dto.ClientId, true);
            if (user == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            }
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (await _favouriteRepository.IsProductFavoritedByUserAsync(dto.ClientId, dto.ProductId))
            {
                return _responseHandler.Failed<bool>(SystemMessages.FAVOURITE_ALREADY_EXISTS);
            }
            var Favourite = _mapper.Map<Favorite>(dto);
            var savedFavourite = await _favouriteRepository.AddAsync(Favourite);
            user.FavoritesCount++;
            await _clientRepository.UpdateAsync(user);

            string cacheKey = $"fav_user_{dto.ClientId}";

            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);

            return _responseHandler.Created<bool>(true);
        }
    }
}
