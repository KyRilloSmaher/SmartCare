using MediatR;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Queries
{
    public record SearchCategoriesByNameAsyncQuery(string name) : IRequest<Response<IEnumerable<CategoryResponseDto>>>;
}
