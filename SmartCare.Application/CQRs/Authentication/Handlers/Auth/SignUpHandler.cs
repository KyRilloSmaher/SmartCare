using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Auth
{
    public class SignUpHandler : IRequestHandler<SignUpAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly LinkGenerator _linkGenerator;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly UserManager<ApplictionUser>_userManager;

        #endregion

        public SignUpHandler(IResponseHandler responseHandler, IClientRepository clientRepository, ITokenService tokenService, IEmailService emailService, IHttpContextAccessor httpContextAccessor, IImageUploaderService imageUploaderService, LinkGenerator linkGenerator, JwtSettings jwtSettings, IMapper mapper, IUrlHelper urlHelper, UserManager<ApplictionUser> userManager)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _imageUploaderService = imageUploaderService;
            _linkGenerator = linkGenerator;
            _jwtSettings = jwtSettings;
            _mapper = mapper;
            _urlHelper = urlHelper;
            _userManager = userManager;
        }


        public async Task<Response<bool>> Handle(SignUpAsyncCommand request, CancellationToken cancellationToken)
        {
            string? uploadedImageUrl = null;

            var dto = request.dto;
            var isEmailExists = await _userManager.FindByEmailAsync(dto.Email);
            if (isEmailExists != null)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_EXISTS);
            var isUserNameExists = await _userManager.FindByNameAsync(dto.UserName);
            if (isUserNameExists != null)
                return _responseHandler.Failed<bool>(SystemMessages.USERNAME_ALREADY_EXISTS);
            var isPhoneNumberExists = await _clientRepository.IsClientPhoneNumberUniqueAsync(dto.PhoneNumber);
            if (!isPhoneNumberExists)
                return _responseHandler.Failed<bool>(SystemMessages.PHONE_ALREADY_EXISTS);

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

                // Map DTO to ApplictionUser
                var user = _mapper.Map<ApplictionUser>(dto);
                user.ProfileImageUrl = uploadedImageUrl;

                // Initialize Client
                user.Client = _mapper.Map<SmartCare.Domain.Entities.Client>(dto);
                user.Client.Addresses = new List<SmartCare.Domain.Entities.Address> { _mapper.Map<SmartCare.Domain.Entities.Address>(dto.Address) };
                user.Client.User = user;

                // Create user in Identity
                var createResult = await _userManager.CreateAsync(user, dto.Password);

                if (!createResult.Succeeded)
                {
                    await _clientRepository.RollbackTransactionAsync();
                    return _responseHandler.Failed<bool>(
                        string.Join(", ", createResult.Errors.Select(e => e.Description))
                    );
                }

                // Add role
                await _userManager.AddToRoleAsync(user, "CLIENT");

                // Generate email confirmation
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);
                var httprequest = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{httprequest.Scheme}://{httprequest.Host}";
                var confirmEmailUrl = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={user.Email}&token={encodedToken}";
                user.EmailConfirmationLink = confirmEmailUrl;
                user.VerificationURLExpiresAt = DateTime.UtcNow.AddHours(24);

                // Save changes to ClientRepository
                await _clientRepository.UpdateAsync(user.Client);

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
    }
}
