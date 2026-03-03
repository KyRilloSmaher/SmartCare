using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using SmartCare.Application.commens;
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

namespace SmartCare.Application.Features.Carts.Commands.RemoveItemFromCart
{
    public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, Response<bool>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveFromCartCommandHandler> _logger;

        #endregion
        public RemoveFromCartCommandHandler(IResponseHandler responseHandler, ILogger<RemoveFromCartCommandHandler> logger, IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<bool>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var cart = await _unitOfWork.Carts.EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var cartItem = await _unitOfWork.Carts.GetCartItemAsync(dto.CartItemId);
            if (cartItem == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            await _unitOfWork.Carts.RemoveCartItemAsync(cartItem);
            //if (!removed)
            //    return _responseHandler.BadRequest<bool>(SystemMessages.SERVER_ERROR);
            await _unitOfWork.Carts.CalculateCartTotalAsync(cart.Id);
            await _unitOfWork.SaveChangesAsync();
            return _responseHandler.Success(true, SystemMessages.ITEM_REMOVED_FROM_CART);
        }
    }
}
