using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.Application.CQRs.Authentication.Commands.Token;
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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Token
{
    public class GetRefreshTokenHandler : IRequestHandler<GetRefreshTokenAsyncCommand, Response<TokenResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;
        private readonly ITokenService _tokenService;

        #endregion

        #region Constructor
        public GetRefreshTokenHandler(IResponseHandler responseHandler, IClientRepository clientRepository, ITokenService tokenService)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _tokenService = tokenService;
        }

        #endregion

        public async Task<Response<TokenResponseDto>> Handle(GetRefreshTokenAsyncCommand request, CancellationToken cancellationToken)
        {
            var  dto = request.dto;
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal == null)
                return _responseHandler.BadRequest<TokenResponseDto>(SystemMessages.BAD_REQUEST);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _clientRepository.GetByIdAsync(userId);

            if (user == null ||
                user.RefreshToken != dto.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.TOKEN_EXPIRED);
            }

            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiryTime();

            await _clientRepository.UpdateAsync(user);

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
