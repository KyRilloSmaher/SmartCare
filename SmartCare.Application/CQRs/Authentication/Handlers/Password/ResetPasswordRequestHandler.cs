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
    public class ResetPasswordRequestHandler : IRequestHandler<ResetPasswordRequestAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;

        #endregion

        #region Constructor
        public ResetPasswordRequestHandler(IResponseHandler responseHandler, IClientRepository clientRepository)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
        }

        #endregion



        public async Task<Response<bool>> Handle(ResetPasswordRequestAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            try
            {
                await _clientRepository.BeginTransactionAsync();

                var user = await _clientRepository.GetByEmailAsync(dto.Email, true);
                if (user == null)
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

                await _clientRepository.RemovePasswordAsync(user);
                await _clientRepository.AddPasswordAsync(user, dto.NewPassword);

                await _clientRepository.CommitTransactionAsync();
                return _responseHandler.Success(true, SystemMessages.PASSWORD_RESET_SUCCESS);
            }
            catch
            {
                await _clientRepository.RollbackTransactionAsync();
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}
