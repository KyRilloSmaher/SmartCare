using Microsoft.AspNetCore.Mvc;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.IServices;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.API.Helpers;
using SmartCare.Application.Handlers.ResponseHandler;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;



namespace SmartCare.API.Controllers
{
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
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
            var result = await _authenticationService.ChangePasswordAsync(userId,dto);
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
        [HttpPost(ApplicationRouting.Authentication.ReSendResetCode)]
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
                return Content(ControllersHelperMethods.HtmlTemplate("Invalid request", "Missing email or token."), "text/html");

            var result = await _authenticationService.ConfirmEmailAsync(dto);

            if (result.Succeeded)
            {
                return Content(ControllersHelperMethods.HtmlTemplate(
                    "Email Confirmed ✅",
                    "Your email has been successfully confirmed! You can now log in to your account."
                ), "text/html");
            }

            return Content(ControllersHelperMethods.HtmlTemplate(
                "Invalid or Expired Link ❌",
                "The confirmation link is invalid or has expired. Please request a new verification email."
            ), "text/html");
        }

        /// <summary>
        /// Re Send Confirmation user email.
        /// </summary>
        [HttpGet(ApplicationRouting.Authentication.ReSendConfirmationEmail)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReSendConfirmationEmailAsync([FromQuery] ReSendConfirmationEmailRequest dto)
        {

            var result = await _authenticationService.ReSendConfirmEmailAsync(dto);

            if (result.Succeeded)
            {
                return Content(ControllersHelperMethods.HtmlTemplate(
                    "Email Confirmed ✅",
                    "Your email has been successfully confirmed! You can now log in to your account."
                ), "text/html");
            }

            return Content(ControllersHelperMethods.HtmlTemplate(
                "Invalid or Expired Link ❌",
                "The confirmation link is invalid or has expired. Please request a new verification email."
            ), "text/html");
        }
        #endregion
    }
}
