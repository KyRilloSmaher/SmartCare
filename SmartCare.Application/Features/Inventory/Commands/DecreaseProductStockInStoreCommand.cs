using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Inventory.Commands
{
    public record DecreaseProductStockInStoreCommand(Guid productId, Guid storeId, int quantityToSubtract) : IRequest<Response<bool>>;
}
