using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Commands.ChangePharmacistBranch
{
    public record ChangePharmacistBranchCommand(string PharmacistId , Guid NewBranchId) : IRequest<Response<bool>>;
}
