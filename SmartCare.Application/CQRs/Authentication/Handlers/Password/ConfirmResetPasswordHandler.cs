using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Authentication.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Password
{
    public class ConfirmResetPasswordHandler : IRequestHandler<ConfirmResetPasswordQuery, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ConfirmResetPasswordHandler> _logger;
        #endregion

        public ConfirmResetPasswordHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            ILogger<ConfirmResetPasswordHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(ConfirmResetPasswordQuery request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            try
            {
                _logger.LogInformation("Confirming password reset code for: {Email}", dto.Email);

                // Fetch user via UnitOfWork
                var user = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", dto.Email);
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
                }

                // Check if OTP exists and is not expired
                if (string.IsNullOrEmpty(user.OTP))
                {
                    _logger.LogWarning("No OTP found for user: {Email}", dto.Email);
                    return _responseHandler.Failed<bool>(SystemMessages.NO_RESET_CODE);
                }

                if (user.OTPExpiryTime < DateTime.UtcNow)
                {
                    _logger.LogWarning("OTP expired for user: {Email}", dto.Email);

                    // Clear expired OTP
                    user.OTP = null;
                    user.OTPExpiryTime = null;
                    user.OTPAttempts = 0;
                    await _unitOfWork.UserManager.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return _responseHandler.Failed<bool>(SystemMessages.RESET_CODE_EXPIRED);
                }

                // Check max attempts
                if (user.OTPAttempts >= 5)
                {
                    _logger.LogWarning("Max OTP attempts reached for user: {Email}", dto.Email);

                    // Clear OTP after max attempts
                    user.OTP = null;
                    user.OTPExpiryTime = null;
                    user.OTPAttempts = 0;
                    await _unitOfWork.UserManager.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return _responseHandler.Failed<bool>(SystemMessages.MAX_ATTEMPTS_REACHED);
                }

                // Verify OTP code
                var isValidCode = BCrypt.Net.BCrypt.Verify(dto.Code, user.OTP);

                if (!isValidCode)
                {
                    // Increment attempts
                    user.OTPAttempts++;
                    await _unitOfWork.UserManager.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning("Invalid OTP attempt {Attempts} for user: {Email}", user.OTPAttempts, dto.Email);

                    return _responseHandler.Failed<bool>(SystemMessages.INVALID_RESET_CODE);
                }

                // Valid code - store in temporary field that reset is confirmed
                user.ResetPasswordConfirmed = true; // Add this field
                user.OTP = null; // Clear OTP after successful verification
                user.OTPExpiryTime = null;
                user.OTPAttempts = 0;

                await _unitOfWork.UserManager.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Password reset code confirmed for: {Email}", dto.Email);

                return _responseHandler.Success(true, SystemMessages.PASSWORD_RESET_CODE_CONFIRMED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming reset code for {Email}", dto.Email);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}