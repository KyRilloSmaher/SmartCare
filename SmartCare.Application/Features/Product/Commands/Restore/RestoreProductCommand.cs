using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Commands.Restore
{
    public record RestoreProductCommand(Guid productId) : IRequest<Response<bool>>;
}
