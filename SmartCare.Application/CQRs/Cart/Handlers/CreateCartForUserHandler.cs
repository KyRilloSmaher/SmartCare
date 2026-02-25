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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCartForUserHandler> _logger;

        #endregion

        public CreateCartForUserHandler(IResponseHandler responseHandler, IUnitOfWork unitOfWork, ILogger<CreateCartForUserHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<Guid>> Handle(CreateCartForUserAsyncCommand request, CancellationToken cancellationToken)
        {
            var userId = request.userId;
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<Guid>(SystemMessages.BAD_REQUEST);

            var existing = await _unitOfWork.Carts.GetActiveCartAsync(userId);
            if (existing != null)
                return _responseHandler.Success(existing.Id, SystemMessages.CART_ALREADY_EXISTS);

            var newCart = await _unitOfWork.Carts.CreateCartAsync(userId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _responseHandler.Success(newCart.Id, SystemMessages.CART_CREATED);
        }
    }
}
