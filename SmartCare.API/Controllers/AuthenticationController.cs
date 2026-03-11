using Microsoft.AspNetCore.Mvc;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.IServices;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.API.Helpers;
using SmartCare.Application.Handlers.ResponseHandler;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SmartCare.API.Services;



namespace SmartCare.API.Controllers
{
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly HtmlTemplateService _templateService;

        public AuthenticationController(IAuthenticationService authenticationService, HtmlTemplateService templateService)
        {
            _authenticationService = authenticationService;
            _templateService = templateService;
        }

        #region Auth Endpoints

        /// <summary>
        /// Register a new client user.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.SignUp)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SignUpAsync([FromForm] SignUpRequest dto)
        {
            var result = await _authenticationService.SignUpAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }



        /// <summary>
        /// Login and retrieve access + refresh tokens.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.Login)]
        [ProducesResponseType(typeof(Response<TokenResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto dto)
        {
            var result = await _authenticationService.LoginAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        ///  LogOut User
        /// </summary>
        [Authorize]
        [HttpPost(ApplicationRouting.Authentication.Logout)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> LogoutAsync()
        {
            // Get current user ID from JWT
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _authenticationService.LogoutAsync(userId);
            return ControllersHelperMethods.FinalResponse(result);

        }
        /// <summary>
        /// Refresh access token using refresh token.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.RefreshToken)]
        [ProducesResponseType(typeof(Response<TokenResponseDto>), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] TokenRequestDto dto)
        {
            var result = await _authenticationService.GetRefreshTokenAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Change password for logged-in user.
        /// </summary>
        [Authorize]
        [HttpPost(ApplicationRouting.Authentication.ChangePassword)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequestDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _authenticationService.ChangePasswordAsync(userId, dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Send reset password code to user email.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.SendResetCode)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendResetPasswordCodeAsync([FromBody] ForgetPasswordRequestDto dto)
        {
            var result = await _authenticationService.SendResetPasswordCodeAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// ReSend reset password code to user email.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.ResendResetCode)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReSendResetPasswordCodeAsync([FromBody] ForgetPasswordRequestDto dto)
        {
            var result = await _authenticationService.ReSendResetPasswordCodeAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Confirm the reset password code before setting new password.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.ConfirmResetPasswordCode)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmResetPasswordCodeAsync([FromBody] ConfirmResetPasswordCodeRequestDto dto)
        {
            var result = await _authenticationService.ConfirmResetPasswordAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Reset user password.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.ResetPassword)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] SetNewPasswordRequestDto dto)
        {
            var result = await _authenticationService.ResetPasswordRequestAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Confirm user email.
        /// </summary>
        [HttpGet(ApplicationRouting.Authentication.ConfirmEmail)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] ConfirmEmailRequest dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Token))
            {
                var html = _templateService.GetHtmlTemplate("InvalidRequest", new Dictionary<string, string>
        {
            { "message", "Missing email or token." }
        });
                return Content(html, "text/html");
            }

            var result = await _authenticationService.ConfirmEmailAsync(dto);
            if (result.Succeeded)
            {
                var html = _templateService.GetHtmlTemplate("EmailConfirmed", new Dictionary<string, string>
        {
            { "message", "Your email has been successfully confirmed! You can now log in to your account." },
            { "loginUrl", $"{ApplicationRouting.Authentication.Login}" }
        });
                return Content(html, "text/html");
            }
            var request = HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var resendLink = $"{baseUrl}/{ApplicationRouting.Authentication.ResendConfirmationEmail}?email={Uri.EscapeDataString(dto.Email)}";

            var htmlFailed = _templateService.GetHtmlTemplate("InvalidConfirmationLink", new Dictionary<string, string>
    {
        { "resendLink", resendLink }
    });
            return Content(htmlFailed, "text/html");
        }

        /// <summary>
        /// Re Send Confirmation user email.
        /// </summary>
        [HttpGet(ApplicationRouting.Authentication.ResendConfirmationEmail)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReSendConfirmationEmailAsync([FromQuery] ReSendConfirmationEmailRequest dto)
        {
            var result = await _authenticationService.ReSendConfirmEmailAsync(dto);

            if (result.Succeeded)
            {
                var html = _templateService.GetHtmlTemplate("VerificationEmailSent", new Dictionary<string, string>
        {
            { "email", dto.Email }
        });
                return Content(html, "text/html");
            }

            var htmlFailed = _templateService.GetHtmlTemplate("InvalidRequest", new Dictionary<string, string>
    {
        { "message", $"We couldn't send a verification email to {dto.Email}. Please try again or contact support." }
    });
            return Content(htmlFailed, "text/html");
        }
        #endregion
    }
}
