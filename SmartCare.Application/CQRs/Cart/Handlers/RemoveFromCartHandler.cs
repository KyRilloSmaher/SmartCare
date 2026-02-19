using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Cart.Commands;
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
    public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartAsyncCommand, Response<bool>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<RemoveFromCartHandler> _logger;

        #endregion
        public RemoveFromCartHandler(IResponseHandler responseHandler, ICartRepository cartRepository, ILogger<RemoveFromCartHandler> logger)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(RemoveFromCartAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var cart = await _cartRepository.EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId);
            if (cartItem == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var removed = await _cartRepository.RemoveCartItemAsync(cartItem);
            if (!removed)
                return _responseHandler.BadRequest<bool>(SystemMessages.SERVER_ERROR);
            await _cartRepository.CalculateCartTotalAsync(cart.Id);
            return _responseHandler.Success(true, SystemMessages.ITEM_REMOVED_FROM_CART);
        }
    }
}
