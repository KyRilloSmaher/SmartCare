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

namespace SmartCare.Application.Features.Carts.Commands.ClearCart
{
    public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Response<bool>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClearCartCommandHandler> _logger;



        #endregion
        public ClearCartCommandHandler(
            IResponseHandler responseHandler,
           
            ILogger<ClearCartCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
           
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<bool>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _unitOfWork.Carts.EnsureCartExistsAsync(request.cartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            await _unitOfWork.Carts.ClearCartAsync(cart.Id);
            cart.ReCalculateTotalPrice();
            await _unitOfWork.SaveChangesAsync();
            return _responseHandler.Success(true, SystemMessages.CART_CLEARED);
        }
    }
}
