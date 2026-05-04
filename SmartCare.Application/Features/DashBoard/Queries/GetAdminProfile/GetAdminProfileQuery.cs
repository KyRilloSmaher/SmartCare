using MediatR;
using SmartCare.Application.DTOs.Admins;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAdminProfile
{
    public record GetAdminProfileQuery(string Id) : IRequest<Response<AdminProfile>>;
}
