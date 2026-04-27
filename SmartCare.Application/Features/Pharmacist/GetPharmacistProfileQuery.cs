using MediatR;
using SmartCare.Application.DTOs.Pharmacist.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Pharmacist
{
    public record GetPharmacistProfileQuery(string UserId) : IRequest<Response<PharmacistProfileDto>>;
}
