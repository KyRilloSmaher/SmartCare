using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.Queries
{
    public record IsPickupCodeValidAsyncQuery(Guid OrderId, string verifyCode) : IRequest<Response<bool>>;
}
