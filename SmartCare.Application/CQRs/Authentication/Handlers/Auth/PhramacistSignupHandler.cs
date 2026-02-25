using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
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
        private readonly IPharmacistRepository _pharmacistRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly LinkGenerator _linkGenerator;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;


        #endregion
        public PhramacistSignupHandler(IResponseHandler responseHandler, IPharmacistRepository pharmacistRepository, IStoreRepository storeRepository, ITokenService tokenService, IEmailService emailService, IHttpContextAccessor httpContextAccessor, IImageUploaderService imageUploaderService, LinkGenerator linkGenerator, JwtSettings jwtSettings, IMapper mapper, IUrlHelper urlHelper)
        {
            _responseHandler = responseHandler;
            _pharmacistRepository = pharmacistRepository;
            _storeRepository = storeRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _imageUploaderService = imageUploaderService;
            _linkGenerator = linkGenerator;
            _jwtSettings = jwtSettings;
            _mapper = mapper;
            _urlHelper = urlHelper;
        }


        public async Task<Response<bool>> Handle(pharmacistSignUpAsyncCommand request, CancellationToken cancellationToken)
        {
            //string? uploadedImageUrl = null;
            //var dto = request.dto;

            //var isEmailExists = await _pharmacistRepository.GetByEmailAsync(dto.Email);
            //if (isEmailExists != null)
            //    return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_EXISTS);

            //var isUserNameExists = await _pharmacistRepository.SearchByNameAsync(dto.userName);
            //if (isUserNameExists != null)
            //    return _responseHandler.Failed<bool>(SystemMessages.USERNAME_ALREADY_EXISTS);

            //var isBranchExists = await _storeRepository.GetStoreByIdAsync(dto.StoreId);
            //if (isBranchExists == null)
            //    return _responseHandler.Failed<bool>(SystemMessages.STORE_NOT_FOUND);

            //var isPhoneNumberUnique = await _pharmacistRepository.IspharmacistPhoneNumberUniqueAsync(dto.PhoneNumber);
            //if (!isPhoneNumberUnique)
            //    return _responseHandler.Failed<bool>(SystemMessages.PHONE_ALREADY_EXISTS);

            //try
            //{
            //    if (dto.ProfileImage is not null)
            //    {
            //        var uploadResult = await _imageUploaderService.UploadImageAsync(dto.ProfileImage, ImageFolder.UserProfiles);
            //        if (uploadResult.Error != null)
            //            return _responseHandler.Failed<bool>(SystemMessages.FILE_UPLOAD_FAILED);

            //        uploadedImageUrl = uploadResult.Url.ToString();
            //    }

            //    await _pharmacistRepository.BeginTransactionAsync();

            //    var pharmacist = _mapper.Map<SmartCare.Domain.Entities.Pharmacist>(dto);
            //    pharmacist.ProfileImageUrl = uploadedImageUrl;

            //    var createResult = await _pharmacistRepository.CreatepharmacistAsync(pharmacist, dto.Password);

            //    if (!createResult.Succeeded)
            //    {
            //        await _pharmacistRepository.RollbackTransactionAsync();
            //        if (!string.IsNullOrEmpty(uploadedImageUrl))
            //            await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

            //        return _responseHandler.Failed<bool>(string.Join(", ", createResult.Errors.Select(e => e.Description)));
            //    }

            //    await _pharmacistRepository.AddToRoleAsync(pharmacist, "Pharmacist");

            //    var token = await _pharmacistRepository.GenerateEmailConfirmationTokenAsync(pharmacist);
            //    var encodedToken = WebUtility.UrlEncode(token);
            //    var httprequest = _httpContextAccessor.HttpContext!.Request;
            //    var baseUrl = $"{httprequest.Scheme}://{httprequest.Host}";
            //    var confirmEmailUrl = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={pharmacist.Email}&token={encodedToken}";

            //    pharmacist.EmailConfirmationLink = confirmEmailUrl;
            //    pharmacist.VerificationURLExpiresAt = DateTime.UtcNow.AddHours(24);

            //    await _emailService.SendConfirmationEmailAsync(pharmacist.Email, confirmEmailUrl);

            //    await _pharmacistRepository.CommitTransactionAsync();

            //    return _responseHandler.Success(true, SystemMessages.SUCCESS);
            //}
            //catch (Exception ex)
            //{
            //    await _pharmacistRepository.RollbackTransactionAsync();

            //    if (!string.IsNullOrEmpty(uploadedImageUrl))
            //        await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

              return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            //}

        }
    }
}
