using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Authentication.Commands.Password;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Application.ExternalServiceInterfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Password
{
    public class SendResetPasswordCodeHandler : IRequestHandler<SendResetPasswordCodeAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<ApplictionUser> _userManager;
        private readonly IEmailService _emailService;
        #endregion

        #region Constructor
        public SendResetPasswordCodeHandler(
            IResponseHandler responseHandler,
            UserManager<ApplictionUser> userManager,
            IEmailService emailService)
        {
            _responseHandler = responseHandler;
            _userManager = userManager;
            _emailService = emailService;
        }
        #endregion

        public async Task<Response<bool>> Handle(SendResetPasswordCodeAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            // Get user via UserManager
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            try
            {
                // Generate OTP
                var OTP = new Random().Next(0, 1_000_000).ToString("D6");
                user.OTP = BCrypt.Net.BCrypt.HashPassword(OTP);

                // Persist OTP
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return _responseHandler.Failed<bool>(
                        string.Join(", ", updateResult.Errors.Select(e => e.Description))
                    );

                // Send OTP email
                await _emailService.SendPasswordResetEmailAsync(
                    user.Email,
                    SystemMessages.SUBJECT_PASSWORD_RESET,
                    OTP
                );

                return _responseHandler.Success(true, SystemMessages.RESET_PASSWORD_CODE_SENT);
            }
            catch
            {
                return _responseHandler.Failed<bool>(SystemMessages.GENERATING_CODE_FAILED);
            }
        }
    }
}