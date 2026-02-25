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
    public class SendResetPasswordCodeHandler : IRequestHandler<SendResetPasswordCodeAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<SendResetPasswordCodeHandler> _logger;
        #endregion

        public SendResetPasswordCodeHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<SendResetPasswordCodeHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(SendResetPasswordCodeAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            try
            {
                _logger.LogInformation("Sending password reset code to: {Email}", dto.Email);

                // Get user via UnitOfWork
                var user = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", dto.Email);
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
                }
                    // Generate OTP
                    var otp = new Random().Next(0, 1_000_000).ToString("D6");
                    var otpHash = BCrypt.Net.BCrypt.HashPassword(otp);

                    // Store OTP in user entity (temporary field)
                    user.OTP = otpHash;
                    user.OTPExpiryTime = DateTime.UtcNow.AddMinutes(15); // Add this field
                    user.OTPAttempts = 0; // Add this field

                    // Persist OTP
                    var updateResult = await _unitOfWork.UserManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to update user with OTP: {Errors}", errors);
                        throw new Exception(errors);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Send OTP email
                    await _emailService.SendPasswordResetEmailAsync(
                        user.Email,
                        SystemMessages.SUBJECT_PASSWORD_RESET,
                        otp
                    );

                    _logger.LogInformation("Password reset code sent to: {Email}", dto.Email);

                    return _responseHandler.Success(true, SystemMessages.RESET_PASSWORD_CODE_SENT);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset code to {Email}", dto.Email);
                return _responseHandler.Failed<bool>(SystemMessages.GENERATING_CODE_FAILED);
            }
        }
    }
}