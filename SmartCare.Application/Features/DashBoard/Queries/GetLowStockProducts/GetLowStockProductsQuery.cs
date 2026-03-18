using MediatR;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetLowStockProducts
{
    public class GetLowStockProductsQuery: IRequest<Response<PaginatedResult<LowStockProductDto>>>
    {
        public Guid? StoreId { get; set; }
        public int Threshold { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }
}
