using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.API.Helpers;
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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Email
{
    public class ReSendConfirmEmailAsyncHandler : IRequestHandler<ReSendConfirmEmailAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Constructor
        public ReSendConfirmEmailAsyncHandler(IResponseHandler responseHandler, IClientRepository clientRepository, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion


        public async Task<Response<bool>> Handle(ReSendConfirmEmailAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var user = await _clientRepository.GetByEmailAsync(dto.Email, true);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            if (user.EmailConfirmed)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_VERIFIED);
            //Generate email confirmation token and link
            var token = await _clientRepository.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var httprequest = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{httprequest.Scheme}://{httprequest.Host}";
            var confirmEmailUrl = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={user.Email}&token={encodedToken}";
            user.EmailConfirmationLink = confirmEmailUrl;
            user.VerificationURLExpiresAt = DateTime.UtcNow.AddHours(24);
            bool success = await _emailService.SendConfirmationEmailAsync(user.Email, confirmEmailUrl);
            return success ? _responseHandler.Success(success, SystemMessages.FAILED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
    }
}
