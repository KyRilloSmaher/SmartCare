using MediatR;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.DTOs.Auth.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Commands.Token
{
    public record GetRefreshTokenAsyncCommand(TokenRequestDto dto) : IRequest<Response<TokenResponseDto>>;

}
