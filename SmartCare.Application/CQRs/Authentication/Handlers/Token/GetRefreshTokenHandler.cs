using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Authentication.Commands.Token;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Interfaces.IServices;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Token
{
    public class GetRefreshTokenHandler : IRequestHandler<GetRefreshTokenAsyncCommand, Response<TokenResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<ApplictionUser> _userManager;
        private readonly ITokenService _tokenService;
        #endregion

        public GetRefreshTokenHandler(IResponseHandler responseHandler, UserManager<ApplictionUser> userManager, ITokenService tokenService)
        {
            _responseHandler = responseHandler;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<Response<TokenResponseDto>> Handle(GetRefreshTokenAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            // Get principal from expired access token
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal == null)
                return _responseHandler.BadRequest<TokenResponseDto>(SystemMessages.BAD_REQUEST);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null ||
                user.RefreshToken != dto.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.TOKEN_EXPIRED);
            }

            // Generate new refresh token and update user
            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiryTime();

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return _responseHandler.Failed<TokenResponseDto>(string.Join(", ", updateResult.Errors));

            var response = new TokenResponseDto
            {
                AccessToken = _tokenService.GenerateAccessToken(principal.Claims),
                RefreshToken = user.RefreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                RefreshTokenExpiresAt = user.RefreshTokenExpiryTime!.Value
            };

            return _responseHandler.Success(response, SystemMessages.TOKEN_GENERATED);
        }
    }
}