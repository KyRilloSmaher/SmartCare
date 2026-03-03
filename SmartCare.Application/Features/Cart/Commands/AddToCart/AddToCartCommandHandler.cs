using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Cart.Extensions;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Application.Messaging;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Carts.Commands.AddToCart
{
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Response<CartItemResponseDto?>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AddToCartCommandHandler> _logger;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IBackgroundJobService _backgroundJobService;
        #endregion
        public AddToCartCommandHandler(
            IResponseHandler responseHandler,
            IMapper mapper,
            ILogger<AddToCartCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IEventPublisherService eventPublisherService,
            IBackgroundJobService backgroundJobService)
        {
            _responseHandler = responseHandler;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _eventPublisherService = eventPublisherService;
            _backgroundJobService = backgroundJobService;
        }
        public async Task<Response<CartItemResponseDto?>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var cart = await _unitOfWork.Carts.EnsureCartExistsAsync(dto.CartId,true);
            if (cart == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var product = await _unitOfWork.Products.EnsureProductExistsAsync(dto.ProductId);
            if (product == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            if (await _unitOfWork.Carts.CheckIfProductExistInCart(dto.CartId, dto.ProductId))
                return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.PRODUCT_ALREADY_IN_CART);
            var availableStock = await _unitOfWork.Inventories.GetTotalStockForProductAsync(product.ProductId);
            if (availableStock < dto.Quantity)
                return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
            // For First we choose default inventory
            var inventory = await _unitOfWork.Inventories.GetAvailableInventoryAsync(product.ProductId, dto.Quantity);
            if (inventory is null)
                return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);

            // map And Set values coming from domain entities
            var cartItem = _mapper.Map<CartItem>(dto);
            cartItem.CartId = cart.Id;
            cartItem.ProductId = product.ProductId;
            cartItem.UnitPrice = product.Price;
            cartItem.SubTotal = product.Price * cartItem.Quantity;
            cartItem.InventoryId = inventory.Id;
            cart.ReCalculateTotalPrice();
            await _unitOfWork.Carts.AddCartItemAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();
            var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
            _backgroundJobService.Enqueue(() => CleanUpCart(cartItem.CartItemId, cart.ClientId));
            return _responseHandler.Success(responseDto, SystemMessages.ADDED_TO_CART);
        }


        public void CleanUpCart(Guid cartItemId , string UserId)
        {
            var cartItem = _unitOfWork.Carts.GetCartItemAsync(cartItemId, true);
            if (cartItem is not null)
            {
                _unitOfWork.Carts.RemoveCartItemAsync(cartItem.Result);
                _eventPublisherService.PublishProductRemovedFromCart(cartItem.Result.CartId, cartItem.Result.ProductId, cartItem.Result.Quantity, UserId);
            }
        }
    }
}
