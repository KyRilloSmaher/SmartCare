using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Auth
{
    public class LogoutHandler : IRequestHandler<LogoutAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;

        #endregion

        public LogoutHandler(IResponseHandler responseHandler, IClientRepository clientRepository)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
        }
        public async Task<Response<bool>> Handle(LogoutAsyncCommand request, CancellationToken cancellationToken)
        {
            var userId = request.userId;
            var user = await _clientRepository.GetByIdAsync(userId, true);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _clientRepository.UpdateSecurityStampAsync(user);
            await _clientRepository.UpdateAsync(user);

            return _responseHandler.Success(true, SystemMessages.LOGOUT_SUCCESS);
        }
    }
}
