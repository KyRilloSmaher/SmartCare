using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.Application.Features.Analytics.Orders.GetOrdersAnalytics
{
    public class GetOrdersAnalyticsQueryHandler
        : IRequestHandler<GetOrdersAnalyticsQuery, Response<OrdersTrendDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetOrdersAnalyticsQueryHandler> _logger;

        public GetOrdersAnalyticsQueryHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<GetOrdersAnalyticsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<OrdersTrendDto>> Handle(GetOrdersAnalyticsQuery request,CancellationToken cancellationToken)
        {

                // Validation — date range
                if (request.StartDate.HasValue && request.StartDate.Value != default &&
                    request.EndDate.HasValue && request.EndDate.Value != default &&
                    request.StartDate.Value > request.EndDate.Value)
                {
                    _logger.LogWarning(
                        "Invalid date range: StartDate {StartDate} is after EndDate {EndDate}",
                        request.StartDate, request.EndDate);
                    return _responseHandler.BadRequest<OrdersTrendDto>(
                        "Start date cannot be after end date.");
                }

                var data = await _unitOfWork.Sales.GetOrdersTrendAsync(
                    request.BranchId,
                    request.interval,
                    request.StartDate,
                    request.EndDate);

                if (data == null || !data.Any())
                {
                    _logger.LogWarning(
                        "No orders trend data found. BranchId: {BranchId}, Interval: {Interval}",
                        request.BranchId, request.interval);
                    return _responseHandler.NotFound<OrdersTrendDto>(
                        "No orders data found for the given filters.");
                }

                var result = new OrdersTrendDto
                {
                    Data = data
                    ,Interval = request.interval
                };

                _logger.LogInformation(
                    "Successfully fetched {Count} trend points. BranchId: {BranchId}, Interval: {Interval}",
                    data.Count, request.BranchId, request.interval);

                return _responseHandler.Success(result);
            }
           
        
    }
}