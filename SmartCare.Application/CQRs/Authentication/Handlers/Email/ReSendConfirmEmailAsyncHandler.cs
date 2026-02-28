using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Authentication.Commands.Email;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System.Text;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Email
{
    public class ReSendConfirmEmailAsyncHandler : IRequestHandler<ReSendConfirmEmailAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReSendConfirmEmailAsyncHandler> _logger;
        #endregion

        #region Constructor
        public ReSendConfirmEmailAsyncHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ReSendConfirmEmailAsyncHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        #endregion

        public async Task<Response<bool>> Handle(ReSendConfirmEmailAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;

            try
            {
                _logger.LogInformation("Resending confirmation email to: {Email}", dto.Email);

                // Get user
                var user = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", dto.Email);
                    return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);
                }

                // Check if email is already confirmed
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("Email already confirmed for user: {UserId}", user.Id);
                    return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_VERIFIED);
                }
                string token = null ;
                var ExistingVerifaction = await _unitOfWork.EmailVerifications.GetLatestByEmailAsync(dto.Email);
                if (ExistingVerifaction is not null && !ExistingVerifaction.IsUsed)
                {
                    token = ExistingVerifaction?.Token;
                    
                }
                else
                {

                    token = await _unitOfWork.UserManager.GenerateEmailConfirmationTokenAsync(user);
                    // Store verification in EmailVerifications table
                    await _unitOfWork.EmailVerifications.AddVerificationAsync(
                        email: user.Email,
                        code: token,
                        validFor: TimeSpan.FromHours(24)
                    );
                }
                
                // Generate new email confirmation token
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                // Build confirmation URL
                    var requestScheme = _httpContextAccessor.HttpContext!.Request.Scheme;
                    var requestHost = _httpContextAccessor.HttpContext.Request.Host;
                    var baseUrl = $"{requestScheme}://{requestHost}";
                    var confirmEmailUrl = $"{baseUrl}/{ApplicationRouting.Authentication.ConfirmEmail}?email={user.Email}&token={encodedToken}";
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Send confirmation email
                    var emailSent = await _emailService.SendConfirmationEmailAsync(user.Email, confirmEmailUrl);

                    if (!emailSent)
                    {
                        _logger.LogError("Failed to send confirmation email to: {Email}", dto.Email);
                        throw new Exception("Email sending failed");
                    }

                    _logger.LogInformation("Confirmation email resent successfully to: {Email}", dto.Email);

                    return _responseHandler.Success(true, SystemMessages.EMAIL_SENT);
             
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending confirmation email to {Email}: {Message}", dto.Email, ex.Message);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}