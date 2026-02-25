using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Interfaces.IServices;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Auth
{
    public class LoginHandler : IRequestHandler<LoginAsyncCommand, Response<TokenResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<ApplictionUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        #endregion

        public LoginHandler(
            IResponseHandler responseHandler,
            UserManager<ApplictionUser> userManager,
            ITokenService tokenService,
            JwtSettings jwtSettings)
        {
            _responseHandler = responseHandler;
            _userManager = userManager;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;
        }

        public async Task<Response<TokenResponseDto>> Handle(LoginAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            // Get user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.INVALID_CREDENTIALS);

            if (!user.EmailConfirmed)
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.EMAIL_NOT_CONFIRMED);

            // Generate claims & tokens
            var claims = await _tokenService.GetClaimsAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiryTime();
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return _responseHandler.Failed<TokenResponseDto>(
                    string.Join(", ", updateResult.Errors)
                );

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