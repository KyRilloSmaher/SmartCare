using MediatR;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Queries
{
    public record GetClientByIdAsyncQuery(string id) : IRequest<Response<ClientResponseDto?>>;
}
