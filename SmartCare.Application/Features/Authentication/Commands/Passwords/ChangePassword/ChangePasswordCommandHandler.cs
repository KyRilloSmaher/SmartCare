using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Authentication.Commands.Password.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;
        #endregion

        public ChangePasswordCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            ILogger<ChangePasswordCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            var dto = request.dto;

            try
            {
                _logger.LogInformation("Changing password for user: {UserId}", userId);

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
                }

                    // Remove old password (if exists) and set new password
                    var removePassResult = await _unitOfWork.UserManager.RemovePasswordAsync(user);
                    if (!removePassResult.Succeeded)
                    {
                        var errors = string.Join(", ", removePassResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to remove password for user {UserId}: {Errors}", userId, errors);
                        throw new Exception(errors);
                    }

                    var addPassResult = await _unitOfWork.UserManager.AddPasswordAsync(user, dto.NewPassword);
                    if (!addPassResult.Succeeded)
                    {
                        var errors = string.Join(", ", addPassResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to add new password for user {UserId}: {Errors}", userId, errors);
                        throw new Exception(errors);
                    }

                    // Update security stamp to invalidate sessions/tokens
                    await _unitOfWork.UserManager.UpdateSecurityStampAsync(user);

                    // Update user
                    await _unitOfWork.UserManager.UpdateAsync(user);


                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

                    return _responseHandler.Success(true, SystemMessages.PASSWORD_CHANGED_SUCCESS);
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return _responseHandler.Failed<bool>(SystemMessages.PASSWORD_CHANGE_FAILED);
            }
        }
    }
}