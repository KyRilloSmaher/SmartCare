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
    public record SearchProductsByCompanyNameQuery(string CompanyName, int pageNumber, int pageSize) : IRequest<Response<PaginatedResult<ProductResponseDtoForClient>>>;
}
