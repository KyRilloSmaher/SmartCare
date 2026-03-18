
using MediatR;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Queries.SearchInProducts
{
    public record SearchQuery(string query) : IRequest<Response<IEnumerable<ProductResponseDtoForClient>>>;
}
