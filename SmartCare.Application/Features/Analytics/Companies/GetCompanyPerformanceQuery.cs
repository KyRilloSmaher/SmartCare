using MediatR;
using SmartCare.Application.DTOs.Analytics.Categories;
using SmartCare.Application.DTOs.Analytics.Companies;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Companies
{
    public class GetCompanyPerformanceQuery : IRequest<Response<List<CompanyPerformanceDto>>>
    {
        public Guid? BranchId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
