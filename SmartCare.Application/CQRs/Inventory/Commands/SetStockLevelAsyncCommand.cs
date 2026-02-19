using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Commands
{
    public record SetStockLevelAsyncCommand(Guid inventoryId, int newQuantity) : IRequest<Response<bool>>;
}
