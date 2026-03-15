using MediatR;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Company.Queries.GetAllForAdmin
{
    public record GetAllCompaniesForAdminQuery : IRequest<Response<IEnumerable<CompanyResponseForAdminDto>>>;
}
