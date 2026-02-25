using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Authentication.Commands.Email;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Email
{
    public class ReSendConfirmEmailAsyncHandler : IRequestHandler<ReSendConfirmEmailAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<ApplictionUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Constructor
        public ReSendConfirmEmailAsyncHandler(
            IResponseHandler responseHandler,
            UserManager<ApplictionUser> userManager,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _responseHandler = responseHandler;
            _userManager = userManager;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public async Task<Response<bool>> Handle(ReSendConfirmEmailAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            // Get user via UserManager
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            if (user.EmailConfirmed)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_VERIFIED);

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            // Build confirmation URL
            var requestScheme = _httpContextAccessor.HttpContext!.Request.Scheme;
            var requestHost = _httpContextAccessor.HttpContext.Request.Host;
            var baseUrl = $"{requestScheme}://{requestHost}";
            var confirmEmailUrl = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={user.Email}&token={encodedToken}";

            // Update user properties
            user.EmailConfirmationLink = confirmEmailUrl;
            user.VerificationURLExpiresAt = DateTime.UtcNow.AddHours(24);

            // Save updated user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return _responseHandler.Failed<bool>(
                    string.Join(", ", updateResult.Errors.Select(e => e.Description))
                );

            // Send confirmation email
            bool emailSent = await _emailService.SendConfirmationEmailAsync(user.Email, confirmEmailUrl);

            return emailSent
                ? _responseHandler.Success(true, SystemMessages.SUCCESS)
                : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
    }
}