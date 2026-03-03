using MediatR;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Rate.Queries
{
    public record GetAllRatesForProductAsyncQuery(Guid Id) : IRequest<Response<IEnumerable<RateResponseDto>>>;
}
