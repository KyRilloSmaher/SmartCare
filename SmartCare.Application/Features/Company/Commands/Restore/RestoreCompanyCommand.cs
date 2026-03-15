using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Company.Commands.Restore
{
    public record RestoreCompanyCommand(Guid Id) : IRequest<Response<bool>>;
}
