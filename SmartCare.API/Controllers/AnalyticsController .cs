using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.Features.Analytics.Categories;
using SmartCare.Application.Features.Analytics.Companies;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "DASHBOARD_ADMIN")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(ApplicationRouting.Analytics.Categories)]
        public async Task<IActionResult> GetCategoryPerformance([FromQuery] Guid branch_id, [FromQuery] DateTime start_date, [FromQuery] DateTime end_date)
        {
            var query = new GetCategoryPerformanceQuery
            {
                BranchId = branch_id,
                StartDate = start_date,
                EndDate = end_date
            };

            var result = await _mediator.Send(query);
            return ControllersHelperMethods.FinalResponse(result);
        }
        [HttpGet(ApplicationRouting.Analytics.Companies)]
        public async Task<IActionResult> GetCompanyPerformance([FromQuery] Guid branch_id, [FromQuery] DateTime start_date, [FromQuery] DateTime end_date)
        {
            var query = new GetCompanyPerformanceQuery
            {
                BranchId = branch_id,
                StartDate = start_date,
                EndDate = end_date
            };

            var result = await _mediator.Send(query);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
