using MediatR;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Company.Commands.Update
{
    public record UpdateCompanyCommand(UpdateCompanyRequest CompanyDto) : IRequest<Response<CompanyResponseDto>>;
}
