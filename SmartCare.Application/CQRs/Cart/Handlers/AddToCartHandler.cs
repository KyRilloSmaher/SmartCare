using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Cart.Commands;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Application.Messaging;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.Application.CQRs.Cart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Cart.Handlers
{
    public class AddToCartHandler : IRequestHandler<AddToCartAsyncCommand, Response<CartItemResponseDto?>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AddToCartHandler> _logger;


        #endregion
        public AddToCartHandler(
            IResponseHandler responseHandler,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IMapper mapper,
            ILogger<AddToCartHandler> logger)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<Response<CartItemResponseDto?>> Handle(AddToCartAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var cart = await _cartRepository.EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var product = await _productRepository.EnsureProductExistsAsync(dto.ProductId);
            if (product == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            if (await _cartRepository.CheckIfProductExistInCart(dto.CartId, dto.ProductId))
                return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.PRODUCT_ALREADY_IN_CART);
            var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);
            if (availableStock < dto.Quantity)
                return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
            // For First we choose default inventory
            var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(product.ProductId, dto.Quantity);
            if (inventoryId == Guid.Empty)
                return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);

            // map And Set values coming from domain entities
            var cartItem = _mapper.Map<CartItem>(dto);
            cartItem.CartId = cart.Id;
            cartItem.ProductId = product.ProductId;
            cartItem.UnitPrice = product.Price;
            cartItem.SubTotal = product.Price * cartItem.Quantity;
            cartItem.InventoryId = inventoryId;

            await _cartRepository.AddCartItemAsync(cartItem);
            await _cartRepository.CalculateCartTotalAsync(cart.Id);
            var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
            return _responseHandler.Success(responseDto, SystemMessages.ADDED_TO_CART);
        }
    }
}
