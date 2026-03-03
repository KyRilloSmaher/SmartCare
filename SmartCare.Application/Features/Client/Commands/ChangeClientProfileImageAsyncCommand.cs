using MediatR;
using SmartCare.Application.DTOs.Client.Requests;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Commands
{
    public record ChangeClientProfileImageAsyncCommand(string UserId, ChangeClientProfileImageRequestDto dto) : IRequest<Response<string>>;
}
