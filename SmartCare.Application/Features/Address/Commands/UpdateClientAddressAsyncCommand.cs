using MediatR;
using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Application.DTOs.Address.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Address.Commands
{
    public record UpdateClientAddressAsyncCommand(string clientId, UpdateAddressRequestDto dto) : IRequest<Response<AddressResponseDto>>;
}
