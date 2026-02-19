using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.Application.CQRs.Authentication.Commands.Email;
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

namespace SmartCare.Application.CQRs.Authentication.Handlers.Email
{
    public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;

        #endregion

        #region Constructor
        public ConfirmEmailHandler(IResponseHandler responseHandler, IClientRepository clientRepository)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
        }

        #endregion


        public async Task<Response<bool>> Handle(ConfirmEmailAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var user = await _clientRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            if (user.EmailConfirmed)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_VERIFIED);
            if (user.VerificationURLExpiresAt < DateTime.UtcNow)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_VERIFICATION_LINK_EXPIRED);

            var success = await _clientRepository.ConfirmEmailAsync(dto.Email, dto.Token);
            var message = success ? SystemMessages.VERIFICATION_SUCCESS : SystemMessages.VERIFICATION_FAILED;

            return success ? _responseHandler.Success(success, message) : _responseHandler.Failed<bool>(message);
        }
    }
}
