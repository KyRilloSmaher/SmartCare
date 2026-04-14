using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Commands.AddAdmin
{
    public record AddAdminCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string Password
    ): IRequest<Response<bool>>;
}
