using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Orders.GetOrdersAnalytics
{
    public class GetOrdersAnalyticsQuery : IRequest<Response<OrdersTrendDto>>
    {
        public Guid? BranchId { get; set; }
        public FilterIntervales interval { get; set; } = FilterIntervales.monthly;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
