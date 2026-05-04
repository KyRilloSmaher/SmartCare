using MediatR;
using SmartCare.Application.DTOs.Pharmacist.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAllPharmacists
{
    public record GetAllPharmacistsQuery(): IRequest<Response<List<PharmacistProfileDto>>>;
}
