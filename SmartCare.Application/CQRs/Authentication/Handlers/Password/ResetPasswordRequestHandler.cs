using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Authentication.Commands.Password;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Password
{
    public class ResetPasswordRequestHandler : IRequestHandler<ResetPasswordRequestAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ResetPasswordRequestHandler> _logger;
        #endregion

        public ResetPasswordRequestHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            ILogger<ResetPasswordRequestHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(ResetPasswordRequestAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            try
            {
                _logger.LogInformation("Resetting password for: {Email}", dto.Email);

                var user = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", dto.Email);
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
                }

                // Check if reset was confirmed
                if (!user.ResetPasswordConfirmed)
                {
                    _logger.LogWarning("Reset password not confirmed for: {Email}", dto.Email);
                    return _responseHandler.Failed<bool>(SystemMessages.RESET_NOT_CONFIRMED);
                }
                    // Remove old password
                    var removePassResult = await _unitOfWork.UserManager.RemovePasswordAsync(user);
                    if (!removePassResult.Succeeded)
                    {
                        var errors = string.Join(", ", removePassResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to remove password: {Errors}", errors);
                        throw new Exception(errors);
                    }

                    // Add new password
                    var addPassResult = await _unitOfWork.UserManager.AddPasswordAsync(user, dto.NewPassword);
                    if (!addPassResult.Succeeded)
                    {
                        var errors = string.Join(", ", addPassResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to add new password: {Errors}", errors);
                        throw new Exception(errors);
                    }

                    // Update security stamp to invalidate existing sessions/tokens
                    await _unitOfWork.UserManager.UpdateSecurityStampAsync(user);

                    // Clear reset confirmation flag
                    user.ResetPasswordConfirmed = false;

                    // Update user
                    await _unitOfWork.UserManager.UpdateAsync(user);


                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Password reset successfully for: {Email}", dto.Email);

                    return _responseHandler.Success(true, SystemMessages.PASSWORD_RESET_SUCCESS);
              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", dto.Email);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}