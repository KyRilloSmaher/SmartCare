using MediatR;
using SmartCare.Application.DTOs.Analytics.Stores.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Stores
{
    public record GetBranchPerformanceQuery (DateTime? StartDate , DateTime? EndDate) : IRequest<Response<List<BranchPerformanceDto>>>;
}
