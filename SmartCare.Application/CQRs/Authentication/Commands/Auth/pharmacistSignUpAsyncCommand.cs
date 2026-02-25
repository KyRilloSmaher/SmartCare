using MediatR;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.DTOs.Pharmacist.Request;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Commands.Auth
{
    public record pharmacistSignUpAsyncCommand(pharmacistSignUpRequestDto dto) : IRequest<Response<bool>>;
}
