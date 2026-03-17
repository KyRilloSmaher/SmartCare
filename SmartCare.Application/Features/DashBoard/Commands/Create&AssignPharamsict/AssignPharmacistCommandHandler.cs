using AutoMapper;
using CloudinaryDotNet.Core;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Authentication.Handlers.Auth;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Domain.IRepositories;
using System.Text;

namespace SmartCare.Application.Features.DashBoard.Commands.Create_AssignPharamsict
{


    public class AssignPharmacistCommandHandler: IRequestHandler<AssignPharmacistCommand, Response<PharmacistResponseDto>>
    {
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly ILogger<AssignPharmacistCommandHandler> _logger;

        public AssignPharmacistCommandHandler(IResponseHandler responseHandler, IUnitOfWork unitOfWork, ITokenService tokenService, IEmailService emailService, IHttpContextAccessor httpContextAccessor, IImageUploaderService imageUploaderService, JwtSettings jwtSettings, IMapper mapper, ILogger<AssignPharmacistCommandHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _imageUploaderService = imageUploaderService;
            _jwtSettings = jwtSettings;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<PharmacistResponseDto>> Handle(AssignPharmacistCommand request, CancellationToken cancellationToken)
        {
            string? uploadedImageUrl = null;
            var dto = request.pharmacistRequest;

            _logger.LogInformation("Starting signup process for email: {Email}", dto.Email);

            // Validation checks
            var isEmailExists = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
            if (isEmailExists != null)
            {
                _logger.LogWarning("Signup failed - Email already exists: {Email}", dto.Email);
                return _responseHandler.Failed<PharmacistResponseDto>(SystemMessages.EMAIL_ALREADY_EXISTS);
            }

            var isUserNameExists = await _unitOfWork.UserManager.FindByNameAsync(dto.UserName);
            if (isUserNameExists != null)
            {
                _logger.LogWarning("Signup failed - Username already exists: {UserName}", dto.UserName);
                return _responseHandler.Failed<PharmacistResponseDto>(SystemMessages.USERNAME_ALREADY_EXISTS);
            }

            var isPhoneNumberExists = await _unitOfWork.Clients.IsClientPhoneNumberUniqueAsync(dto.PhoneNumber);
            if (!isPhoneNumberExists)
            {
                _logger.LogWarning("Signup failed - Phone already exists: {Phone}", dto.PhoneNumber);
                return _responseHandler.Failed<PharmacistResponseDto>(SystemMessages.PHONE_ALREADY_EXISTS);
            }
            var store = await _unitOfWork.Stores.GetByIdAsync(request.StoreId);
            if (store == null)
                return _responseHandler.Failed<PharmacistResponseDto>("Store not found");
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Upload profile image
                if (dto.ProfileImage is not null)
                {
                    var uploadResult = await _imageUploaderService.UploadImageAsync(dto.ProfileImage, ImageFolder.UserProfiles);

                    if (uploadResult.Error != null)
                    {
                        _logger.LogWarning("Image upload failed for email: {Email}", dto.Email);
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return _responseHandler.Failed<PharmacistResponseDto>(SystemMessages.FILE_UPLOAD_FAILED);
                    }

                    uploadedImageUrl = uploadResult.Url.ToString();
                }

                // Map user
                var user = _mapper.Map<ApplictionUser>(dto);
                user.ProfileImageUrl = uploadedImageUrl;
                user.EmailConfirmed = false;

                user.Pharmacist = _mapper.Map<SmartCare.Domain.Entities.Pharmacist>(dto);
                user.Pharmacist.StoreId = store.Id;
                // Create Identity user
                var createResult = await _unitOfWork.UserManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    _logger.LogWarning("User creation failed for {Email}: {Errors}",
                        dto.Email,
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));

                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                        await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                    return _responseHandler.Failed<PharmacistResponseDto>(
                        string.Join(", ", createResult.Errors.Select(e => e.Description))
                    );
                }

                // Add role
                var roleResult = await _unitOfWork.UserManager.AddToRoleAsync(user, "PHARMACIST");
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Role assignment failed for {Email}", dto.Email);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return _responseHandler.Failed<PharmacistResponseDto>(SystemMessages.FAILED);
                }

                // Generate confirmation token
                var token = await _unitOfWork.UserManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var httpRequest = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{httpRequest.Scheme}://{httpRequest.Host}";
                var confirmEmailUrl = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={user.Email}&token={encodedToken}";

                // Domain operations
                await _unitOfWork.EmailVerifications.AddVerificationAsync(
                    user.Email,
                    token,
                    TimeSpan.FromHours(24)
                );
                // Save domain changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Signup transaction committed successfully for {Email}", dto.Email);

                // Send email after commit
                await _emailService.SendConfirmationEmailAsync(user.Email, confirmEmailUrl);
                var response = new PharmacistResponseDto
                {
                    Id = user.Id,
                    FullName = user.FirstName + user.LastName,
                    PharmacistEmail = user.Email,
                    PharmacistUserName = user.UserName,
                    BranchId = store.Id,
                    Licence = user.Pharmacist.LicenseNumber,
                    Phone = user.PhoneNumber
                };
                return _responseHandler.Success(response, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup transaction for {Email}", dto.Email);

                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                if (!string.IsNullOrEmpty(uploadedImageUrl))
                {
                    try
                    {
                        await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);
                    }
                    catch (Exception imgEx)
                    {
                        _logger.LogError(imgEx, "Failed to cleanup image after signup failure for {Email}", dto.Email);
                    }
                }

                return _responseHandler.Failed<PharmacistResponseDto>(SystemMessages.FAILED);
            }
        }
    }
}
