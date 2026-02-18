using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Commands
{
    public record MarkOrderPaymentAsCashCommand(Guid OrderId) : IRequest<Response<bool>>;
}
