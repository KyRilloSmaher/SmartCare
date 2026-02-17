using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Commands
{
    public record HandlePaymentAsyncCommand(SmartCare.Domain.Entities.Order order) : IRequest<Unit>;
}
