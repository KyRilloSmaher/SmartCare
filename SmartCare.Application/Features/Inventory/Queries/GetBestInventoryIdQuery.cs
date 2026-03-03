using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Queries
{
    public record GetBestInventoryIdQuery(Guid productId, int quantityRequired) : IRequest<Response<Guid>>;
}
