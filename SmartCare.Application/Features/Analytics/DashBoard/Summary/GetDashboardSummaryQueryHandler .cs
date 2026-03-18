using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.Application.Features.Analytics.DashBoard.Summary
{
    public class GetDashboardSummaryQueryHandler
        : IRequestHandler<GetDashboardSummaryQuery, Response<DashboardSummaryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetDashboardSummaryQueryHandler> _logger;

        public GetDashboardSummaryQueryHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<GetDashboardSummaryQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<DashboardSummaryDto>> Handle(
            GetDashboardSummaryQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var summary = await _unitOfWork.Sales.GetDashboardSummaryAsync(
                    request.BranchId,
                    request.StartDate,
                    request.EndDate);

                return _responseHandler.Success(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get dashboard summary for branch {BranchId}",
                    request.BranchId);

                return _responseHandler.Failed<DashboardSummaryDto>(
                    "Failed to retrieve dashboard summary.");
            }
        }
    }
}