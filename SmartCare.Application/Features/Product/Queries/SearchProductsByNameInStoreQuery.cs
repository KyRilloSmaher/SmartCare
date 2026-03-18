using MediatR;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Queries
{
    public record SearchProductsByNameInStoreQuery(string ProductName, Guid StoreId)
                         : IRequest<Response<IEnumerable<ProductResponseDtoForPharmacist>>>;
}
