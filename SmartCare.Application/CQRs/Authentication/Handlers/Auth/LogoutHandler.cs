using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Auth
{
    public class LogoutHandler : IRequestHandler<LogoutAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<ApplictionUser> _userManager;
        #endregion

        public LogoutHandler(IResponseHandler responseHandler, UserManager<ApplictionUser> userManager)
        {
            _responseHandler = responseHandler;
            _userManager = userManager;
        }

        public async Task<Response<bool>> Handle(LogoutAsyncCommand request, CancellationToken cancellationToken)
        {
            var userId = request.userId;

            // Fetch user via Identity
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            // Clear refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            // Update security stamp and user
            await _userManager.UpdateSecurityStampAsync(user);
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return _responseHandler.Failed<bool>(string.Join(", ", updateResult.Errors));

            return _responseHandler.Success(true, SystemMessages.LOGOUT_SUCCESS);
        }
    }
}