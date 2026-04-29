using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.Commands
{
    public record AcceptDeliveryCommand(Guid OrderId, string DeliveryPersonId)
       : IRequest<Response<bool>>;
}
