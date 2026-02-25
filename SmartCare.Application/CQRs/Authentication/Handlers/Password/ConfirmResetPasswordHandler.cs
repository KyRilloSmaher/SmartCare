using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Authentication.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Password
{
    public class ConfirmResetPasswordHandler : IRequestHandler<ConfirmResetPasswordQuery, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<ApplictionUser> _userManager;
        #endregion

        public ConfirmResetPasswordHandler(IResponseHandler responseHandler, UserManager<ApplictionUser> userManager)
        {
            _responseHandler = responseHandler;
            _userManager = userManager;
        }

        public async Task<Response<bool>> Handle(ConfirmResetPasswordQuery request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            // Fetch user via Identity
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            // Verify OTP code
            var isValidCode = BCrypt.Net.BCrypt.Verify(dto.Code, user.OTP);
            var message = isValidCode
                ? SystemMessages.PASSWORD_RESET_CODE_CONFIRMED
                : SystemMessages.INVALID_RESET_CODE;

            return isValidCode? _responseHandler.Success(isValidCode, message) : _responseHandler.Failed<bool>(message);
        }
    }
}