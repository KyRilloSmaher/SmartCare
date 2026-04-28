using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Commands.ConfirmPharmacistEmail
{
    public record ConfirmPharmacistEmailCommand (string id) : MediatR.IRequest<SmartCare.Application.Handlers.ResponseHandler.Response<bool>>;
}
