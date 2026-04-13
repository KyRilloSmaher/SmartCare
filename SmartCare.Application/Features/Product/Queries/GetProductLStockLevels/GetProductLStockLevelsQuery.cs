using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Queries.GetProductLStockLevels
{
    public class GetProductLStockLevelsQuery: IRequest<Response<IEnumerable<ProductLevelInStore>>>
    {
        public Guid ProductId { get; set; }
    }
}
