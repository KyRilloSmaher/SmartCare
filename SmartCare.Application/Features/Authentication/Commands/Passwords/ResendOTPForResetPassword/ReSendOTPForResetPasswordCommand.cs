using MediatR;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Authentication.Commands.Passwords.ResendOTPForResetPassword
{
    public record ReSendOTPForResetPasswordCommand(ForgetPasswordRequestDto dto) : IRequest<Response<bool>>;
}
