using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.DashBoard.Summary
{
    public record GetDashboardSummaryQuery(Guid? BranchId , DateTime? StartDate , DateTime? EndDate): IRequest<Response<DashboardSummaryDto>>;
    
}
