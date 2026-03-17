using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.DashBoard
{
    public record GetDashboardSummaryQuery(Guid? BranchId , DateTime? StartDate , DateTime? EndDate): IRequest<Response<DashboardSummaryDto>>;
    
}
