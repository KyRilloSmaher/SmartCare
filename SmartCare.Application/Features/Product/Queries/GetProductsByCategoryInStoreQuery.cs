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
    public record GetProductsByCategoryInStoreQuery(Guid CategoryId, Guid StoreId, int PageNumber, int PageSize)
    : IRequest<Response<PaginatedResult<ProductResponseDtoForPharmacist>>>;
}
