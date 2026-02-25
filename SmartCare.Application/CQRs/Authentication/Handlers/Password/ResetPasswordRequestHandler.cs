using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Authentication.Commands.Password;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Password
{
    public class ResetPasswordRequestHandler : IRequestHandler<ResetPasswordRequestAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<ApplictionUser> _userManager;
        #endregion

        public ResetPasswordRequestHandler(IResponseHandler responseHandler, UserManager<ApplictionUser> userManager)
        {
            _responseHandler = responseHandler;
            _userManager = userManager;
        }

        public async Task<Response<bool>> Handle(ResetPasswordRequestAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            try
            {
                // Remove old password (if exists)
                var removePassResult = await _userManager.RemovePasswordAsync(user);
                if (!removePassResult.Succeeded)
                    return _responseHandler.Failed<bool>(
                        string.Join(", ", removePassResult.Errors));

                // Add new password
                var addPassResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
                if (!addPassResult.Succeeded)
                    return _responseHandler.Failed<bool>(
                        string.Join(", ", addPassResult.Errors));

                // Update security stamp to invalidate existing sessions/tokens
                await _userManager.UpdateSecurityStampAsync(user);

                return _responseHandler.Success(true, SystemMessages.PASSWORD_RESET_SUCCESS);
            }
            catch
            {
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}