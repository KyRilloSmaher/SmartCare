using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.Application.CQRs.Authentication.Queries;
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

namespace SmartCare.Application.CQRs.Authentication.Handlers.Password
{
    public class ConfirmResetPasswordHandler : IRequestHandler<ConfirmResetPasswordQuery, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;

        #endregion

        #region Constructor
        public ConfirmResetPasswordHandler(IResponseHandler responseHandler, IClientRepository clientRepository)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
        }

        #endregion
        public async Task<Response<bool>> Handle(ConfirmResetPasswordQuery request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var user = await _clientRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            var Hashed_OTP = BCrypt.Net.BCrypt.HashPassword(dto.Code);
            var isValidCode = BCrypt.Net.BCrypt.Verify(dto.Code, user.OTP);
            var message = isValidCode
                ? SystemMessages.PASSWORD_RESET_CODE_CONFIRMED
                : SystemMessages.INVALID_RESET_CODE;

            return _responseHandler.Success(isValidCode, message);
        }
    }
}
