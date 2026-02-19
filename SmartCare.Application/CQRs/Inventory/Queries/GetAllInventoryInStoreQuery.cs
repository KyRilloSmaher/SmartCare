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
    public record GetAllInventoryInStoreQuery(Guid storeId, int pageNumber, int pageSize) : IRequest<Response<PaginatedResult<InventoryAdminResponseDto>>>;
}
