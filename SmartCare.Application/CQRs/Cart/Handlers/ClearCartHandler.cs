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
    public class ClearCartHandler : IRequestHandler<ClearCartAsyncCommand, Response<bool>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClearCartHandler> _logger;



        #endregion
        public ClearCartHandler(
            IResponseHandler responseHandler,
           
            ILogger<ClearCartHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
           
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<bool>> Handle(ClearCartAsyncCommand request, CancellationToken cancellationToken)
        {
            var cart = await _unitOfWork.Carts.EnsureCartExistsAsync(request.cartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var cartItems = await _unitOfWork.Carts.GetCartItemsAsync(cart.Id);
            if (!cartItems.Any())
                return _responseHandler.Success(true, SystemMessages.CART_CLEARED);

            foreach (var item in cartItems)
                await _unitOfWork.Carts.RemoveCartItemAsync(item);
            await _unitOfWork.Carts.CalculateCartTotalAsync(cart.Id);
            await _unitOfWork.SaveChangesAsync();
            return _responseHandler.Success(true, SystemMessages.CART_CLEARED);
        }
    }
}
