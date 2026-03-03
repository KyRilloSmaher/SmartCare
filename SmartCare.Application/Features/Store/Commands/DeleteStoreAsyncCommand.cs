using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Store.Commands
{
    public record DeleteStoreAsyncCommand(Guid Id) : IRequest<Response<bool>>;
}
