using MediatR;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Company.Commands
{
    public record ChangeCompanyLogoAsyncCommand(Guid Id, ChangeCompanyLogoRequestDto CompanyDto) : IRequest<Response<string>>;
}
