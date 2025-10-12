using Microsoft.AspNetCore.Mvc;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.IServices;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.API.Helpers;
using SmartCare.Application.Handlers.ResponseHandler;



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
        /// Confirm user email.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.ConfirmEmail)]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] ConfirmEmailRequest dto)
        {
            var result = await _authenticationService.ConfirmEmailAsync(dto);
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
        public async Task<IActionResult> RefreshTokenAsync([FromBody] TokenRequestDto dto)
        {
            var result = await _authenticationService.GetRefreshTokenAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Change password for logged-in user.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.ChangePassword)]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequestDto dto)
        {
            var result = await _authenticationService.ChangePasswordAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Send reset password code to user email.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.SendResetCode)]
        public async Task<IActionResult> SendResetPasswordCodeAsync([FromBody] SetNewPasswordRequestDto dto)
        {
            var result = await _authenticationService.SendResetPasswordCodeAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Confirm the reset password code before setting new password.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.ConfirmResetPasswordCode)]
        public async Task<IActionResult> ConfirmResetPasswordCodeAsync([FromBody] ConfirmResetPasswordCodeRequestDto dto)
        {
            var result = await _authenticationService.ConfirmResetPasswordAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Reset user password.
        /// </summary>
        [HttpPost(ApplicationRouting.Authentication.ResetPassword)]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ForgetPasswordRequestDto dto)
        {
            var result = await _authenticationService.ResetPasswordRequestAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        #endregion
    }
}

