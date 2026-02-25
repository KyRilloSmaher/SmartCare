using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using TokenRequestDto = SmartCare.Application.DTOs.Auth.Requests.TokenRequestDto;

namespace SmartCare.Application.IServices
{
    public interface IAuthenticationService
    {
        // Token management
        Task<Response<TokenResponseDto>> GetRefreshTokenAsync(TokenRequestDto dto);

        // Email confirmation
        Task<Response<bool>> ConfirmEmailAsync(ConfirmEmailRequest dto);
        Task<Response<bool>> ReSendConfirmEmailAsync(ReSendConfirmationEmailRequest dto);

        // Password management
        Task<Response<bool>> SendResetPasswordCodeAsync(ForgetPasswordRequestDto dto);
        Task<Response<bool>> ReSendResetPasswordCodeAsync(ForgetPasswordRequestDto dto);
        Task<Response<bool>> ConfirmResetPasswordAsync(ConfirmResetPasswordCodeRequestDto dto);
        Task<Response<bool>> ResetPasswordRequestAsync(SetNewPasswordRequestDto dto);
        Task<Response<bool>> ChangePasswordAsync(string UserId, ChangePasswordRequestDto dto);

        // Authentication
        Task<Response<TokenResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<Response<bool>> SignUpAsync(SignUpRequest dto);
        Task<Response<bool>> LogoutAsync(string userId);
    }
}