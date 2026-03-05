using MediatR;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Queries
{
    public record GetStockOfProductInStoreQuery(Guid productId, Guid storeId) : IRequest<Response<InventoryUserResponseDto>?>;
}
