using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartCare.Application.DTOs.Analytics.Sales;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Sales.Revenue
{
    public record GetRevenueAnalyticsQuery(Guid? BranchId, DateTime? StartDate , DateTime? EndDate,FilterIntervales interval = FilterIntervales.monthly) : IRequest<Response<RevenueAnalyticsDto>>;
}
