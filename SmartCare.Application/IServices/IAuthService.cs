using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.DTOs.Auth.Requests;
using TokenRequestDto = SmartCare.Application.DTOs.Auth.Requests.TokenRequestDto;
namespace SmartCare.Application.IServices
{
    public interface IAuthenticationService
    {
        Task<Response<TokenResponseDto>> GetRefreshTokenAsync(TokenRequestDto dto);
        Task<Response<bool>> ConfirmEmailAsync(ConfirmEmailRequest dto);
        Task<Response<bool>> SendResetPasswordCodeAsync(ForgetPasswordRequestDto dto);
        Task<Response<bool>> ResetPasswordRequestAsync(SetNewPasswordRequestDto dto);
        Task<Response<bool>> ConfirmResetPasswordAsync(ConfirmResetPasswordCodeRequestDto dto);
        Task<Response<bool>> ChangePasswordAsync(string UserId ,ChangePasswordRequestDto dto);
        Task<Response<TokenResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<Response<bool>> SignUpAsync(SignUpRequest dto);
    }
}
