using MediatR;
using SmartCare.Application.DTOs.Analytics.Sales;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Sales.SalesChannel
{
    public record GetSalesChannelAnalyticsQuery(Guid? BranchId , DateTime? StartDate , DateTime? EndDate): IRequest<Response<List<SalesChannelDto>>>;
    
}
