using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Authentication.Commands.Email;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Email
{
    public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ConfirmEmailHandler> _logger;
        #endregion

        public ConfirmEmailHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ConfirmEmailHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(ConfirmEmailAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            try
            {
                _logger.LogInformation("Starting email confirmation for {Email}", dto.Email);

                // Fetch user via Identity
                var user = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email confirmation: {Email}", dto.Email);
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
                }

                // Check if email is already confirmed
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("Email already confirmed for user: {UserId}", user.Id);
                    return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_VERIFIED);
                }

                // Get valid verification from EmailVerifications table
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));

                var verification = await _unitOfWork.EmailVerifications.GetValidVerificationAsync(dto.Email, decodedToken);
                if (verification == null)
                {
                    _logger.LogWarning("Invalid or expired verification token for email: {Email}", dto.Email);
                    return _responseHandler.Failed<bool>(SystemMessages.INVALID_TOKEN);
                }

                    // Confirm email using Identity
                    var result = await _unitOfWork.UserManager.ConfirmEmailAsync(user, decodedToken);

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError("Email confirmation failed for user {UserId}. Errors: {Errors}",
                            user.Id, errors);

                        throw new Exception(errors);
                    }
                    // Update user
                    user.EmailConfirmed = true;
                    verification.markUsed();
                    await _unitOfWork.UserManager.UpdateAsync(user);
                    // Create A Cart For Client
                    await _unitOfWork.Carts.CreateCartAsync(user.Id);
                    // Save all changes atomically
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Email confirmed successfully for user: {UserId}", user.Id);

                    return _responseHandler.Success(true, SystemMessages.VERIFICATION_SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for {Email}: {Message}", dto.Email, ex.Message);
                return _responseHandler.Failed<bool>(SystemMessages.VERIFICATION_FAILED);
            }
        }
    }
}