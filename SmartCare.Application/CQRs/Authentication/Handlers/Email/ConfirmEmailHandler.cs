using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Authentication.Commands.Email;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Email
{
    public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<ApplictionUser> _userManager;
        #endregion

        public ConfirmEmailHandler(IResponseHandler responseHandler, UserManager<ApplictionUser> userManager)
        {
            _responseHandler = responseHandler;
            _userManager = userManager;
        }

        public async Task<Response<bool>> Handle(ConfirmEmailAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            // Fetch user via Identity
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            if (user.EmailConfirmed)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_VERIFIED);

            if (user.VerificationURLExpiresAt < DateTime.UtcNow)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_VERIFICATION_LINK_EXPIRED);

            // Confirm email using Identity
            var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
            var message = result.Succeeded ? SystemMessages.VERIFICATION_SUCCESS : SystemMessages.VERIFICATION_FAILED;

            return result.Succeeded ? _responseHandler.Success(true, message) : _responseHandler.Failed<bool>(message);
        }
    }
}