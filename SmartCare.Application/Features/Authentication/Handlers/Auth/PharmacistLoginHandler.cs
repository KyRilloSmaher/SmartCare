using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.DTOs.Auth.Responses;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Auth
{
    public class PharmacistLoginHandler : IRequestHandler<PharmacistLoginCommand, Response<TokenResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;

        #endregion

        public PharmacistLoginHandler(IResponseHandler responseHandler, IUnitOfWork unitOfWork, ITokenService tokenService, JwtSettings jwtSettings, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;
            _mapper = mapper;
        }
        public async Task<Response<TokenResponseDto>> Handle(PharmacistLoginCommand request, CancellationToken cancellationToken)
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

            // 3. Verify the user exists in the Pharmacists table
            // We check the repository using the Identity User's Id

            //if (!Guid.TryParse(user.Id, out Guid pharmacistGuid))
            //{
            //    return _responseHandler.Unauthorized<TokenResponseDto>("Invalid User ID format.");
            //}

            var pharmacist = await _unitOfWork.Pharmacists.GetByUserIdAsync(user.Id,true);

            if (pharmacist == null)
            {
                return _responseHandler.Unauthorized<TokenResponseDto>("Access denied. This account is not registered as a Pharmacist.");
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
            user.Pharmacist = pharmacist;
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
