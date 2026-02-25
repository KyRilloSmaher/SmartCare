using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Authentication.Commands.Token;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Domain.IRepositories;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRefreshTokenHandler> _logger;
        #endregion

        public GetRefreshTokenHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IMapper mapper,
            ILogger<GetRefreshTokenHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<TokenResponseDto>> Handle(GetRefreshTokenAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            try
            {
                _logger.LogInformation("Attempting to refresh token");

                // Get principal from expired access token
                var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
                if (principal == null)
                {
                    _logger.LogWarning("Invalid token principal from expired token");
                    return _responseHandler.BadRequest<TokenResponseDto>(SystemMessages.INVALID_TOKEN);
                }

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token claims");
                    return _responseHandler.BadRequest<TokenResponseDto>(SystemMessages.INVALID_TOKEN);
                }

                // Get user 
                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.USER_NOT_FOUND);
                }

                // Validate refresh token
                if (user.RefreshToken != dto.RefreshToken)
                {
                    _logger.LogWarning("Refresh token mismatch for user: {UserId}", userId);
                    return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.INVALID_REFRESH_TOKEN);
                }

                if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token expired for user: {UserId}, Expired at: {ExpiryTime}",
                        userId, user.RefreshTokenExpiryTime);
                    return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.REFRESH_TOKEN_EXPIRED);
                }

                    // Generate new refresh token
                    var newRefreshToken = _tokenService.GenerateRefreshToken();
                    var newRefreshTokenExpiry = _tokenService.GetRefreshTokenExpiryTime();

                    // Store old refresh token for audit if needed
                    var oldRefreshToken = user.RefreshToken;

                    // Update user with new refresh token
                    user.RefreshToken = newRefreshToken;
                    user.RefreshTokenExpiryTime = newRefreshTokenExpiry;


                    var updateResult = await _unitOfWork.UserManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to update user during token refresh: {Errors}", errors);
                        throw new Exception(errors);
                    }

                    

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Generate new access token
                    var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);

                    // Get user roles
                    var roles = await _unitOfWork.UserManager.GetRolesAsync(user);

                    var response = new TokenResponseDto
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,
                        AccessTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                        RefreshTokenExpiresAt = newRefreshTokenExpiry,
                       
                    };

                    _logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);

                    return _responseHandler.Success(response, SystemMessages.TOKEN_REFRESHED_SUCCESS);
             
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token for user");
                return _responseHandler.Failed<TokenResponseDto>(SystemMessages.FAILED);
            }
        }
    }
}