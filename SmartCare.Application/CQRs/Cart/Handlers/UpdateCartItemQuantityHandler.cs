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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateCartItemQuantityHandler> _logger;

        #endregion

        public UpdateCartItemQuantityHandler(IResponseHandler responseHandler, IMapper mapper, ILogger<UpdateCartItemQuantityHandler> logger, IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<CartItemResponseDto?>> Handle(UpdateCartItemQuantityAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var cart = await _unitOfWork.Carts.EnsureCartExistsAsync(dto.CartId);
            if (cart is null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var cartItem = await _unitOfWork.Carts.GetCartItemAsync(dto.CartItemId);
            if (cartItem is null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_ITEM_NOT_EXIST);

            var product = await _unitOfWork.Products.EnsureProductExistsAsync(cartItem.ProductId);
            if (product is null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            var quantityDifference = dto.NewQuantity - cartItem.Quantity;

            if (quantityDifference > 0)
            {
                var availableStock =
                    await _unitOfWork.Inventories.GetTotalStockForProductAsync(product.ProductId);

                if (availableStock < quantityDifference)
                    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                var inventory = await _unitOfWork.Inventories.GetAvailableInventoryAsync(product.ProductId, dto.NewQuantity);
                if (inventory is null)
                    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
            }

            cartItem.Quantity = dto.NewQuantity;
            cartItem.SubTotal = cartItem.UnitPrice * dto.NewQuantity;
            await _unitOfWork.Carts.CalculateCartTotalAsync(cart.Id);
            await _unitOfWork.SaveChangesAsync();
            var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
            return _responseHandler.Success(responseDto, SystemMessages.CART_UPDATED);
        }
    }
}
