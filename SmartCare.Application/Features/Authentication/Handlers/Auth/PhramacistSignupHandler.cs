using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
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
    public class PhramacistSignupHandler : IRequestHandler<pharmacistSignUpAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly LinkGenerator _linkGenerator;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly ILogger<SignUpHandler> _logger;

        #endregion

        public PhramacistSignupHandler(IResponseHandler responseHandler, IUnitOfWork unitOfWork, ITokenService tokenService, IEmailService emailService, IHttpContextAccessor httpContextAccessor, IImageUploaderService imageUploaderService, LinkGenerator linkGenerator, JwtSettings jwtSettings, IMapper mapper, IUrlHelper urlHelper, ILogger<SignUpHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _imageUploaderService = imageUploaderService;
            _linkGenerator = linkGenerator;
            _jwtSettings = jwtSettings;
            _mapper = mapper;
            _urlHelper = urlHelper;
            _logger = logger;
        }



        public async Task<Response<bool>> Handle(pharmacistSignUpAsyncCommand request, CancellationToken cancellationToken)
        {
            string? uploadedImageUrl = null;
            var dto = request.dto;

            // Validation checks
            var isBranchIdExists = await _unitOfWork.Stores.GetByIdAsync(dto.StoreId);
            if (isBranchIdExists == null)
                return _responseHandler.Failed<bool>(SystemMessages.STORE_NOT_FOUND);

            var isEmailExists = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
            if (isEmailExists != null)
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_EXISTS);

            var isUserNameExists = await _unitOfWork.UserManager.FindByNameAsync(dto.userName);
            if (isUserNameExists != null)
                return _responseHandler.Failed<bool>(SystemMessages.USERNAME_ALREADY_EXISTS);

            var isPhoneNumberExists = await _unitOfWork.Pharmacists.IsPharmacistPhoneNumberUniqueAsync(dto.PhoneNumber);
            if (!isPhoneNumberExists)
                return _responseHandler.Failed<bool>(SystemMessages.PHONE_ALREADY_EXISTS);

            var isLicenseNumberUnique = await _unitOfWork.Pharmacists.IsPharmacistLicenseNumberUniqueAsync(dto.LicenseNumber);
            if (!isLicenseNumberUnique)
                return _responseHandler.Failed<bool>(SystemMessages.LICENSE_NUMBER_ALREADY_EXISTS);
            if (dto.ProfileImage is not null)
             {
                var uploadResult = await _imageUploaderService.UploadImageAsync(dto.ProfileImage, ImageFolder.UserProfiles);
                if (uploadResult.Error != null)
                   return _responseHandler.Failed<bool>(SystemMessages.FILE_UPLOAD_FAILED);
                uploadedImageUrl = uploadResult.Url.ToString();
              }



            try
            {

                // Map DTO to ApplictionUser
                var user = _mapper.Map<ApplictionUser>(dto);
                user.ProfileImageUrl = uploadedImageUrl ?? string.Empty;
                user.EmailConfirmed = false;

                // Initialize Pharmacist
                user.Pharmacist = _mapper.Map<SmartCare.Domain.Entities.Pharmacist>(dto);
              
                user.Pharmacist.User = user;
                // Create user in Identity
                var createResult = await _unitOfWork.UserManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    // Clean up uploaded image
                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                        await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                    return _responseHandler.Failed<bool>(
                        string.Join(", ", createResult.Errors.Select(e => e.Description))
                    );
                }

                // Add role
                await _unitOfWork.UserManager.AddToRoleAsync(user, "PHARMACIST");

                // Generate email confirmation token

                var token = await _unitOfWork.UserManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var httpRequest = _httpContextAccessor.HttpContext!.Request;
                var scheme = httpRequest.Headers["X-Forwarded-Proto"].FirstOrDefault()
                             ?? httpRequest.Scheme;
                var host = httpRequest.Headers["X-Forwarded-Host"].FirstOrDefault()
                           ?? httpRequest.Host.ToUriComponent();
                var baseUrl = $"{scheme}://{host}";
                var encodedEmail = Uri.EscapeDataString(user.Email!);
                var confirmEmailUrl = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={encodedEmail}&token={encodedToken}";




                // Store email verification
                await _unitOfWork.EmailVerifications.AddVerificationAsync(
                    email: user.Email,
                    code: token,
                    validFor: TimeSpan.FromHours(24)
                );


                // Save all changes atomically in one transaction
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Send confirmation email (after successful save)
                await _emailService.SendConfirmationEmailAsync(user.Email, confirmEmailUrl);

                return _responseHandler.Success(true, SystemMessages.SUCCESS);
                  }
            catch (Exception ex)
            {
                // Clean up uploaded image on failure
                if (!string.IsNullOrEmpty(uploadedImageUrl))
                {
                    try
                    {
                        await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);
                    }
                    catch
                    {
                        // Log but don't throw
                    }
                }

                // Log exception
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }

        }
    }
}
