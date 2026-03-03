using MediatR;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Product.Queries
{
    public record GetDetailsOfProductByNameQuery(string NameEn) : IRequest<Response<ProductResponseDtoForClient>>;
}
