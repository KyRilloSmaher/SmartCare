using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Analytics.Categories;
using SmartCare.Application.DTOs.Analytics.Companies;
using SmartCare.Application.DTOs.Analytics.Sales;
using SmartCare.Application.DTOs.Analytics.Stores.Response;
using SmartCare.Application.Features.Analytics.Categories;
using SmartCare.Application.Features.Analytics.Clients;
using SmartCare.Application.Features.Analytics.Companies;
using SmartCare.Application.Features.Analytics.DashBoard;
using SmartCare.Application.Features.Analytics.DashBoard.Summary;
using SmartCare.Application.Features.Analytics.Orders.GetOrdersAnalytics;
using SmartCare.Application.Features.Analytics.Orders.GetOrderStatusAnalytics;
using SmartCare.Application.Features.Analytics.Sales.Revenue;
using SmartCare.Application.Features.Analytics.Sales.SalesChannel;
using SmartCare.Application.Features.Analytics.Stores;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Projection_Models;

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
        [ProducesResponseType(typeof(Response<List<CategoryPerformanceDto>>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(Response<List<CompanyPerformanceDto>>), StatusCodes.Status200OK)]
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

        [HttpGet(ApplicationRouting.Analytics.Stores)]
        [ProducesResponseType(typeof(Response<List<BranchPerformanceDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBranchesBenchmark([FromQuery] DateTime? start_date,[FromQuery] DateTime? end_date)
        {
            var query = new GetBranchPerformanceQuery(start_date, end_date);
            var result = await _mediator.Send(query);
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpGet(ApplicationRouting.Analytics.SalesChannels)]
        [ProducesResponseType(typeof(Response<List<SalesChannelDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSalesChannels([FromQuery] Guid? branch_id, [FromQuery] DateTime? start_date,[FromQuery] DateTime? end_date)
        {
            var result = await _mediator.Send(new GetSalesChannelAnalyticsQuery(branch_id, start_date, end_date));
            return ControllersHelperMethods.FinalResponse(result);
        }
        [HttpGet(ApplicationRouting.Analytics.Revenue)]
        [ProducesResponseType(typeof(Response<RevenueAnalyticsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueAnalytics([FromQuery] Guid? branch_id,[FromQuery] FilterIntervales interval = FilterIntervales.monthly, [FromQuery] DateTime? start_date = null, [FromQuery] DateTime? end_date = null)
        {
            var result = await _mediator.Send(new GetRevenueAnalyticsQuery(
                 branch_id,
                 start_date,
                 end_date,
                 interval
            ));

            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpGet(ApplicationRouting.Analytics.Summary)]
        [ProducesResponseType(typeof(Response<DashboardSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] Guid? branch_id,[FromQuery] DateTime? start_date,[FromQuery] DateTime? end_date)
        {
            var result = await _mediator.Send(new GetDashboardSummaryQuery(branch_id, start_date,end_date));
            return ControllersHelperMethods.FinalResponse(result);
        }
        [HttpGet(ApplicationRouting.Analytics.Clients)]
        [ProducesResponseType(typeof(Response<ClientAnalyticsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientAnalytics([FromQuery] Guid? branch_id,[FromQuery] FilterIntervales interval = FilterIntervales.monthly ,[FromQuery] DateTime? start_date = null,[FromQuery] DateTime? end_date = null)
        {
            var result = await _mediator.Send(new GetClientAnalyticsQuery( branch_id, end_date, start_date,interval ));
             return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpGet(ApplicationRouting.Analytics.Orders)]
        [ProducesResponseType(typeof(Response<OrdersTrendDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersAnalytics([FromQuery] Guid? branch_id, [FromQuery] FilterIntervales interval = FilterIntervales.daily, [FromQuery] DateTime? start_date = null,[FromQuery] DateTime? end_date = null)
        {
            var result = await _mediator.Send(new GetOrdersAnalyticsQuery
            {
                BranchId = branch_id,
                interval = interval,
                StartDate = start_date,
                EndDate = end_date
            });

            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpGet(ApplicationRouting.Analytics.orderStatus)]
        [ProducesResponseType(typeof(Response<OrderStatusDistributionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersAnalytics([FromQuery] GetOrderStatusAnalyticsQuery query)
        {
            var result = await _mediator.Send(query);
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpGet(ApplicationRouting.Analytics.CategoryChannels)]
        [ProducesResponseType(typeof(Response<CategoryChannelDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryChannelsAsync([FromRoute] Guid categoryId , Guid? branch_id , DateTime From ,DateTime To )
        {
            var result = await _mediator.Send(new CategoryChannelsQuery(categoryId,From ,To, branch_id));
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
