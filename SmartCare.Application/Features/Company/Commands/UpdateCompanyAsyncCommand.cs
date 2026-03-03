using MediatR;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Company.Commands
{
    public record UpdateCompanyAsyncCommand(Guid Id, UpdateCompanyRequest CompanyDto) : IRequest<Response<CompanyResponseDto>>;
}
