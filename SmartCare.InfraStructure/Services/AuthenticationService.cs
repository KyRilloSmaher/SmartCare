using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Interfaces.IServices;
using System.Net;
using System.Security.Claims;

namespace SmartCare.InfraStructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<ApplictionUser> _userManager;
        private readonly SignInManager<ApplictionUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly LinkGenerator _linkGenerator;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public AuthenticationService(
            IResponseHandler responseHandler,
            UserManager<ApplictionUser> userManager,
            SignInManager<ApplictionUser> signInManager,
            ITokenService tokenService,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            IImageUploaderService imageUploaderService,
            LinkGenerator linkGenerator,
            JwtSettings jwtSettings)
        {
            _responseHandler = responseHandler;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _imageUploaderService = imageUploaderService;
            _linkGenerator = linkGenerator;
            _jwtSettings = jwtSettings;
        }
        #endregion

        #region Authentication

        public async Task<Response<TokenResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.INVALID_CREDENTIALS);

            if (!user.EmailConfirmed)
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.EMAIL_NOT_CONFIRMED);

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.INVALID_CREDENTIALS);

            var claims = await _tokenService.GetClaimsAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiryTime();
            await _userManager.UpdateAsync(user);

            return _responseHandler.Success(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddHours(_jwtSettings.AccessTokenLifetimeHours),
                RefreshTokenExpiresAt = user.RefreshTokenExpiryTime!.Value
            }, SystemMessages.LOGIN_SUCCESS);
        }

        public async Task<Response<bool>> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateSecurityStampAsync(user);
            await _userManager.UpdateAsync(user);

            return _responseHandler.Success(true, SystemMessages.LOGOUT_SUCCESS);
        }

        public async Task<Response<bool>> SignUpAsync(SignUpRequest dto)
        {
            var isEmailExists = await _userManager.FindByEmailAsync(dto.Email);
            if (isEmailExists != null)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_EXISTS);

            var isUserNameExists = await _userManager.FindByNameAsync(dto.UserName);
            if (isUserNameExists != null)
                return _responseHandler.Failed<bool>(SystemMessages.USERNAME_ALREADY_EXISTS);

            string? uploadedImageUrl = null;
            if (dto.ProfileImage != null)
            {
                var uploadResult = await _imageUploaderService.UploadImageAsync(dto.ProfileImage, ImageFolder.UserProfiles);
                if (uploadResult.Error != null)
                    return _responseHandler.Failed<bool>(SystemMessages.FILE_UPLOAD_FAILED);
                uploadedImageUrl = uploadResult.Url.ToString();
            }

            var user = _mapper.Map<ApplictionUser>(dto);
            user.ProfileImageUrl = uploadedImageUrl;

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                if (!string.IsNullOrEmpty(uploadedImageUrl))
                    await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                return _responseHandler.Failed<bool>(
                    string.Join(", ", createResult.Errors.Select(e => e.Description))
                );
            }

            await _userManager.AddToRoleAsync(user, "CLIENT");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            user.EmailConfirmationLink = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={user.Email}&token={encodedToken}";
            user.VerificationURLExpiresAt = DateTime.UtcNow.AddHours(24);

            await _userManager.UpdateAsync(user);
            await _emailService.SendConfirmationEmailAsync(user.Email, user.EmailConfirmationLink);

            return _responseHandler.Success(true, SystemMessages.SUCCESS);
        }

        #endregion

        #region Password Management

        public async Task<Response<bool>> SendResetPasswordCodeAsync(ForgetPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            var OTP = new Random().Next(0, 1_000_000).ToString("D6");
            user.OTP = BCrypt.Net.BCrypt.HashPassword(OTP);
            await _userManager.UpdateAsync(user);
            await _emailService.SendPasswordResetEmailAsync(user.Email, SystemMessages.SUBJECT_PASSWORD_RESET, OTP);

            return _responseHandler.Success(true, SystemMessages.RESET_PASSWORD_CODE_SENT);
        }

        public async Task<Response<bool>> ConfirmResetPasswordAsync(ConfirmResetPasswordCodeRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            bool isValidCode = BCrypt.Net.BCrypt.Verify(dto.Code, user.OTP);
            var message = isValidCode ? SystemMessages.PASSWORD_RESET_CODE_CONFIRMED : SystemMessages.INVALID_RESET_CODE;

            return _responseHandler.Success(isValidCode, message);
        }

        public async Task<Response<bool>> ResetPasswordRequestAsync(SetNewPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!resetResult.Succeeded)
                return _responseHandler.Failed<bool>(string.Join(", ", resetResult.Errors.Select(e => e.Description)));

            return _responseHandler.Success(true, SystemMessages.PASSWORD_RESET_SUCCESS);
        }

        public async Task<Response<bool>> ChangePasswordAsync(string userId, ChangePasswordRequestDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                return _responseHandler.Failed<bool>(string.Join(", ", result.Errors.Select(e => e.Description)));

            return _responseHandler.Success(true, SystemMessages.PASSWORD_RESET_SUCCESS);
        }

        #endregion

        #region Email Confirmation

        public async Task<Response<bool>> ConfirmEmailAsync(ConfirmEmailRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            if (user.EmailConfirmed)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_VERIFIED);

            var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
            return result.Succeeded
                ? _responseHandler.Success(true, SystemMessages.VERIFICATION_SUCCESS)
                : _responseHandler.Failed<bool>(SystemMessages.VERIFICATION_FAILED);
        }

        public async Task<Response<bool>> ReSendConfirmEmailAsync(ReSendConfirmationEmailRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            if (user.EmailConfirmed)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_VERIFIED);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            user.EmailConfirmationLink = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={user.Email}&token={encodedToken}";
            user.VerificationURLExpiresAt = DateTime.UtcNow.AddHours(24);
            await _userManager.UpdateAsync(user);

            bool success = await _emailService.SendConfirmationEmailAsync(user.Email, user.EmailConfirmationLink);
            return success ? _responseHandler.Success(true, SystemMessages.SUCCESS) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }

        public Task<Response<TokenResponseDto>> GetRefreshTokenAsync(TokenRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<Response<bool>> ReSendResetPasswordCodeAsync(ForgetPasswordRequestDto dto)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}