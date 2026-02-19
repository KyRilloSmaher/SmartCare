using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.DTOs.Auth.Responses;
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
    public class LoginHandler : IRequestHandler<LoginAsyncCommand, Response<TokenResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        #endregion

        public LoginHandler(IResponseHandler responseHandler, IClientRepository clientRepository, ITokenService tokenService, JwtSettings jwtSettings)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;
        }


        public async Task<Response<TokenResponseDto>> Handle(LoginAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var user = await _clientRepository.GetByEmailAsync(dto.Email);
            if (user == null || !await _clientRepository.CheckPasswordAsync(user, dto.Password))
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.INVALID_CREDENTIALS);
            if (!user.EmailConfirmed)
            {
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.EMAIL_NOT_CONFIRMED);
            }
            var claims = await _tokenService.GetClaimsAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiryTime();

            await _clientRepository.UpdateAsync(user);

            var response = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddHours(_jwtSettings.AccessTokenLifetimeHours),
                RefreshTokenExpiresAt = user.RefreshTokenExpiryTime!.Value
            };

            return _responseHandler.Success(response, SystemMessages.LOGIN_SUCCESS);
        }
    }
}
