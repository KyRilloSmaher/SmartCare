using MediatR;
using SmartCare.Application.DTOs.Pharmacist.Response;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAllNonConfirmedPharmacist
{
    public record GetAllNonConfirmedPharmacistQuery() : IRequest<Response<List<PharmacistProfileDto>>>;
}
