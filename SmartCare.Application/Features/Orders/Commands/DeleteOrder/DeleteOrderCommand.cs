using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.Commands.DeleteOrder
{
    public record DeleteOrderCommand(Guid orderId) : IRequest<Response<bool>>;
}
