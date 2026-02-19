using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.Application.CQRs.Authentication.Commands.Password;
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
    public class SendResetPasswordCodeHandler : IRequestHandler<SendResetPasswordCodeAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;
        private readonly IEmailService _emailService;

        #endregion

        #region Constructor
        public SendResetPasswordCodeHandler(IResponseHandler responseHandler, IClientRepository clientRepository, IEmailService emailService)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _emailService = emailService;
        }

        #endregion
        public async Task<Response<bool>> Handle(SendResetPasswordCodeAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            try
            {
                await _clientRepository.BeginTransactionAsync();

                var user = await _clientRepository.GetByEmailAsync(dto.Email, true);
                if (user == null)
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
                var OTP = new Random().Next(0, 1_000_000).ToString("D6");
                user.OTP = BCrypt.Net.BCrypt.HashPassword(OTP);
                await _clientRepository.UpdateAsync(user);

                await _emailService.SendPasswordResetEmailAsync(
                    user.Email,
                    SystemMessages.SUBJECT_PASSWORD_RESET,
                    OTP);

                await _clientRepository.CommitTransactionAsync();

                return _responseHandler.Success(true, SystemMessages.RESET_PASSWORD_CODE_SENT);
            }
            catch
            {
                await _clientRepository.RollbackTransactionAsync();
                return _responseHandler.Failed<bool>(SystemMessages.GENERATING_CODE_FAILED);
            }
        }
    }
}
