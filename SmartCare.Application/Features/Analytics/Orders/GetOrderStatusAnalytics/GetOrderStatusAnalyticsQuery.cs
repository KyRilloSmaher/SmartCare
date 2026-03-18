using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Orders.GetOrderStatusAnalytics
{
    public class GetOrderStatusAnalyticsQuery
     : IRequest<Response<OrderStatusDistributionDto>>
    {
        public Guid? BranchId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
