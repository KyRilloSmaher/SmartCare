using MediatR;
using SmartCare.Application.DTOs.Rates.Requests;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Rate.Commands
{
    public record UpdateRateAsyncCommand(string userId, UpdateRateRequestDto Dto) : IRequest<Response<RateResponseDto>>;
}
