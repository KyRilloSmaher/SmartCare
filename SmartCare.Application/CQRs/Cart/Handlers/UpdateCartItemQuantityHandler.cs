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
using SmartCare.Domain.IRepositories;
using SmartCare.Application.CQRs.Cart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Cart.Handlers
{
    public class UpdateCartItemQuantityHandler : IRequestHandler<UpdateCartItemQuantityAsyncCommand, Response<CartItemResponseDto?>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateCartItemQuantityHandler> _logger;

        #endregion

        public UpdateCartItemQuantityHandler(IResponseHandler responseHandler, ICartRepository cartRepository, IProductRepository productRepository, IInventoryRepository inventoryRepository, IMapper mapper, ILogger<UpdateCartItemQuantityHandler> logger)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<Response<CartItemResponseDto?>> Handle(UpdateCartItemQuantityAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var cart = await _cartRepository.EnsureCartExistsAsync(dto.CartId);
            if (cart is null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId);
            if (cartItem is null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_ITEM_NOT_EXIST);

            var product = await _productRepository.EnsureProductExistsAsync(cartItem.ProductId);
            if (product is null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            var quantityDifference = dto.NewQuantity - cartItem.Quantity;

            if (quantityDifference > 0)
            {
                var availableStock =
                    await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);

                if (availableStock < quantityDifference)
                    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(product.ProductId, dto.NewQuantity);
                if (inventoryId == Guid.Empty)
                    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
            }

            cartItem.Quantity = dto.NewQuantity;
            cartItem.SubTotal = cartItem.UnitPrice * dto.NewQuantity;

            await _cartRepository.UpdateItemCartAsync(cartItem);
            await _cartRepository.CalculateCartTotalAsync(cart.Id);

            var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
            return _responseHandler.Success(responseDto, SystemMessages.CART_UPDATED);
        }
    }
}
