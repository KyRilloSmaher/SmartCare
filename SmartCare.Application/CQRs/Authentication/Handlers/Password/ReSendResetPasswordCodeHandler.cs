using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Authentication.Commands.Password;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Password
{
    public class ReSendResetPasswordCodeHandler : IRequestHandler<ReSendResetPasswordCodeAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReSendResetPasswordCodeHandler> _logger;
        #endregion

        public ReSendResetPasswordCodeHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<ReSendResetPasswordCodeHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(ReSendResetPasswordCodeAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            try
            {
                _logger.LogInformation("Resending password reset code to: {Email}", dto.Email);

                // Fetch the user via UnitOfWork
                var user = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", dto.Email);
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
                }

                // Rate limiting - check if code was sent recently
                if (user.OTPExpiryTime > DateTime.UtcNow.AddMinutes(-2))
                {
                    var secondsRemaining = (int)(user.OTPExpiryTime.Value - DateTime.UtcNow).TotalSeconds;
                    _logger.LogInformation("Rate limit hit for password reset: {Email}", dto.Email);

                    return _responseHandler.Failed<bool>(
                        string.Format(SystemMessages.PASSWORD_RESET_RATE_LIMIT, secondsRemaining)
                    );
                }

                    // Generate new OTP
                    var otp = new Random().Next(0, 1_000_000).ToString("D6");
                    var otpHash = BCrypt.Net.BCrypt.HashPassword(otp);

                    // Update user with new OTP
                    user.OTP = otpHash;
                    user.OTPExpiryTime = DateTime.UtcNow.AddMinutes(15);
                    user.OTPAttempts = 0;
                    user.ResetPasswordConfirmed = false;

                    // Persist changes
                    var updateResult = await _unitOfWork.UserManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to update user with new OTP: {Errors}", errors);
                        throw new Exception(errors);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Send email
                    await _emailService.SendPasswordResetEmailAsync(
                        user.Email,
                        SystemMessages.SUBJECT_PASSWORD_RESET,
                        otp
                    );

                    _logger.LogInformation("Password reset code resent to: {Email}", dto.Email);

                    return _responseHandler.Success(true, SystemMessages.RESET_PASSWORD_CODE_SENT);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending password reset code to {Email}", dto.Email);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}