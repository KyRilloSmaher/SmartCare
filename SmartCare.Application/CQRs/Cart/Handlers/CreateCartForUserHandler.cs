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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Cart.Handlers
{
    public class CreateCartForUserHandler : IRequestHandler<CreateCartForUserAsyncCommand, Response<Guid>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CreateCartForUserHandler> _logger;

        #endregion

        public CreateCartForUserHandler(IResponseHandler responseHandler, ICartRepository cartRepository, ILogger<CreateCartForUserHandler> logger)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<Response<Guid>> Handle(CreateCartForUserAsyncCommand request, CancellationToken cancellationToken)
        {
            var userId = request.userId;
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<Guid>(SystemMessages.BAD_REQUEST);

            var existing = await _cartRepository.GetActiveCartAsync(userId);
            if (existing != null)
                return _responseHandler.Success(existing.Id, SystemMessages.CART_ALREADY_EXISTS);

            var newCart = await _cartRepository.CreateCartAsync(userId);
            return _responseHandler.Success(newCart.Id, SystemMessages.CART_CREATED);
        }
    }
}
