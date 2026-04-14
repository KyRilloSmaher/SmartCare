using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
namespace SmartCare.Application.Features.Product.Queries.PeopleAlsoBought
{
    public record PeopleAlsoBoughtQuery(Guid ProductId , int topN) : IRequest<Response<List<ProductResponseDtoForClient>>>;
}
