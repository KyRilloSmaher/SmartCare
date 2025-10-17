using AutoMapper;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Domain.IRepositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Web;
using SmartCare.API.Helpers;

namespace SmartCare.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        #endregion

        #region Constructor
        public AuthenticationService(
            IResponseHandler responseHandler,
            IClientRepository clientRepository,
            ITokenService tokenService,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            IImageUploaderService imageUploaderService,
            LinkGenerator linkGenerator,
            IUrlHelper urlHelper)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _imageUploaderService = imageUploaderService;
            _linkGenerator = linkGenerator;
            _urlHelper = urlHelper;
        }
        #endregion

        #region Token Management

        public async Task<Response<TokenResponseDto>> GetRefreshTokenAsync(TokenRequestDto dto)
        {
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

        #endregion

        #region Email Confirmation

        public async Task<Response<bool>> ConfirmEmailAsync(ConfirmEmailRequest dto)
        {

            var success = await _clientRepository.ConfirmEmailAsync(dto.Email, dto.Token);
            var message = success ? SystemMessages.VERIFICATION_SUCCESS : SystemMessages.VERIFICATION_FAILED;

            return success ? _responseHandler.Success(success, message) : _responseHandler.Failed<bool>(message);
        }
        public async Task<Response<bool>> ReSendConfirmEmailAsync(ReSendConfirmationEmailRequest dto)
        {
            var user = await _clientRepository.GetByEmailAsync(dto.Email, true);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            if (user.EmailConfirmed)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_VERIFIED);
            //Generate email confirmation token and link
            var token = await _clientRepository.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var confirmEmailUrl = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={user.Email}&token={encodedToken}";
            bool success = await _emailService.SendConfirmationEmailAsync(user.Email, confirmEmailUrl);
            return success ? _responseHandler.Success(success,SystemMessages.FAILED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
        #endregion

        #region Password Management

        public async Task<Response<bool>> SendResetPasswordCodeAsync(ForgetPasswordRequestDto dto)
        {
            try
            {
                await _clientRepository.BeginTransactionAsync();

                var user = await _clientRepository.GetByEmailAsync(dto.Email, true);
                if (user == null)
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

                user.Code = new Random().Next(0, 1_000_000).ToString("D6");
                await _clientRepository.UpdateAsync(user);

                await _emailService.SendPasswordResetEmailAsync(
                    user.Email,
                    SystemMessages.SUBJECT_PASSWORD_RESET,
                    user.Code);

                await _clientRepository.CommitTransactionAsync();

                return _responseHandler.Success(true, SystemMessages.RESET_PASSWORD_CODE_SENT);
            }
            catch
            {
                await _clientRepository.RollbackTransactionAsync();
                return _responseHandler.Failed<bool>(SystemMessages.GENERATING_CODE_FAILED);
            }
        }


        public async Task<Response<bool>> ReSendResetPasswordCodeAsync(ForgetPasswordRequestDto dto)
        {
            var user = await _clientRepository.GetByEmailAsync(dto.Email, true);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
            user.Code = new Random().Next(0, 1_000_000).ToString("D6");
            await _clientRepository.UpdateAsync(user);
            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                SystemMessages.SUBJECT_PASSWORD_RESET,
                user.Code);

            return _responseHandler.Success(true, SystemMessages.RESET_PASSWORD_CODE_SENT);
        }

        public async Task<Response<bool>> ConfirmResetPasswordAsync(ConfirmResetPasswordCodeRequestDto dto)
        {
            var user = await _clientRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            var isValidCode = user.Code == dto.Code;
            var message = isValidCode
                ? SystemMessages.PASSWORD_RESET_CODE_CONFIRMED
                : SystemMessages.INVALID_RESET_CODE;

            return _responseHandler.Success(isValidCode, message);
        }

        public async Task<Response<bool>> ResetPasswordRequestAsync(SetNewPasswordRequestDto dto)
        {
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

        public async Task<Response<bool>> ChangePasswordAsync(string UserId ,ChangePasswordRequestDto dto)
        {
            try
            {
                await _clientRepository.BeginTransactionAsync();

                var user = await _clientRepository.GetByIdAsync(UserId, true);
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

        #endregion

        #region Authentication

        public async Task<Response<TokenResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _clientRepository.GetByEmailAsync(dto.Email);
            if (user == null || !await _clientRepository.CheckPasswordAsync(user, dto.Password))
                return _responseHandler.Unauthorized<TokenResponseDto>(SystemMessages.INVALID_CREDENTIALS);

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
                AccessTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                RefreshTokenExpiresAt = user.RefreshTokenExpiryTime!.Value
            };

            return _responseHandler.Success(response, SystemMessages.LOGIN_SUCCESS);
        }

        public async Task<Response<bool>> SignUpAsync(SignUpRequest dto)
        {
            string? uploadedImageUrl = null;

            try
            {
                //  Upload profile image
                if (dto.ProfileImage is not null)
                {
                    var uploadResult = await _imageUploaderService.UploadImageAsync(dto.ProfileImage, ImageFolder.UserProfiles);

                    if (uploadResult.Error != null)
                        return _responseHandler.Failed<bool>(SystemMessages.FILE_UPLOAD_FAILED);

                    uploadedImageUrl = uploadResult.Url.ToString();
                }

                await _clientRepository.BeginTransactionAsync();

                var user = _mapper.Map<Client>(dto);
                user.ProfileImageUrl = uploadedImageUrl;

                var address = _mapper.Map<Address>(dto.Address);
                user.Addresses = new List<Address> { address };

                //Create user account
                var createResult = await _clientRepository.CreateClientAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    await _clientRepository.RollbackTransactionAsync();
                    return _responseHandler.Failed<bool>(
                        string.Join(", ", createResult.Errors.Select(e => e.Description))
                    );
                }

                await _clientRepository.AddToRoleAsync(user, "CLIENT");

                //Generate email confirmation token and link
                var token = await _clientRepository.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);
                var request = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var confirmEmailUrl = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={user.Email}&token={encodedToken}";
                await _emailService.SendConfirmationEmailAsync(user.Email, confirmEmailUrl);
                await _clientRepository.CommitTransactionAsync();

                return _responseHandler.Success(true, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            { 
                await _clientRepository.RollbackTransactionAsync();

                if (!string.IsNullOrEmpty(uploadedImageUrl))
                    await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }


        #endregion

        
    }
}
