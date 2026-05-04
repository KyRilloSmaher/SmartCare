using MediatR;
using SmartCare.Application.DTOs.Admins;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAllAdmins
{
    public class GetAllAdminsQuery : IRequest<Response<List<AdminProfile>>>;
}
