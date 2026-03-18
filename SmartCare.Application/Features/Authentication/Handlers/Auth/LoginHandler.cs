using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Auth
{
    public class LoginHandler : IRequestHandler<LoginAsyncCommand, Response<TokenResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        #endregion

        public LoginHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            JwtSettings jwtSettings,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;
            _mapper = mapper;
        }

        public async Task<Response<TokenResponseDto>> Handle(LoginAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            // Get user by email
            var user = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);

            // Validate user credentials
            if (user == null || !await _unitOfWork.UserManager.CheckPasswordAsync(user, dto.Password))
            {
                // Increment failed attempt for existing user
                if (user != null)
                    await _unitOfWork.UserManager.AccessFailedAsync(user);

                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.INVALID_CREDENTIALS);
            }
            var roles = await _unitOfWork.UserManager.GetRolesAsync(user);
            if (roles != null) {
                if (roles.Contains("PHARMACIST"))
                {
                    var pharmacist = await _unitOfWork.Pharmacists.GetByUserIdAsync(user.Id);

                    user.Pharmacist = pharmacist;
                }
            }
            // Check if email is confirmed
            if (!user.EmailConfirmed)
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.EMAIL_NOT_CONFIRMED);

            // Check if account is locked
            if (await _unitOfWork.UserManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _unitOfWork.UserManager.GetLockoutEndDateAsync(user);
                var minutesRemaining = (lockoutEnd - DateTimeOffset.UtcNow)?.Minutes ?? 0;
                return _responseHandler.Unauthorized<TokenResponseDto>(
                    string.Format(SystemMessages.ACCOUNT_LOCKED, minutesRemaining)
                );
            }

            // Generate claims & tokens
            var claims = await _tokenService.GetClaimsAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user with new tokens
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiryTime();
            await _unitOfWork.UserManager.ResetAccessFailedCountAsync(user);


            // Update user
            await _unitOfWork.UserManager.UpdateAsync(user);

          
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Prepare response
            var response = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddHours(_jwtSettings.AccessTokenLifetimeHours),
                RefreshTokenExpiresAt = user.RefreshTokenExpiryTime!.Value,
            };

            return _responseHandler.Success(response, SystemMessages.LOGIN_SUCCESS);
        }
    }
}