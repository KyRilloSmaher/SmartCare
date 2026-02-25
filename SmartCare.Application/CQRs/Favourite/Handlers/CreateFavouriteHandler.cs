using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Favourite.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Favourite.Handlers
{
    public class CreateFavouriteHandler : IRequestHandler<CreateFavouriteAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly IRedisCacheService _redisCacheService;
        private readonly string tag = CacheConstants.Favourite;
        #endregion

        public CreateFavouriteHandler(
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

        public async Task<Response<bool>> Handle(CreateFavouriteAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var user = await _unitOfWork.Clients.GetByIdAsync(dto.ClientId, true);
            if (user == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            }

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.PRODUCT_NOT_FOUND);
            }

            if (await _unitOfWork.Favourites.IsProductFavoritedByUserAsync(dto.ClientId, dto.ProductId))
            {
                return _responseHandler.Failed<bool>(SystemMessages.FAVOURITE_ALREADY_EXISTS);
            }

            var Favourite = _mapper.Map<Favorite>(dto);
            var savedFavourite = await _unitOfWork.Favourites.AddAsync(Favourite);
            user.FavoritesCount++;

            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string cacheKey = $"fav_user_{dto.ClientId}";
            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);

            return _responseHandler.Created<bool>(true);
        }
    }
}