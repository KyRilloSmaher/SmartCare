using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Company.Commands
{
    public record DeleteCompanyAsyncCommand(Guid Id) : IRequest<Response<bool>>;
}
