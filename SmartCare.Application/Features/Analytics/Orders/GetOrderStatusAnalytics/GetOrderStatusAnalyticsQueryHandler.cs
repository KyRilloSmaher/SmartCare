using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.Application.Features.Analytics.Orders.GetOrderStatusAnalytics
{
    public class GetOrderStatusAnalyticsQueryHandler
        : IRequestHandler<GetOrderStatusAnalyticsQuery, Response<OrderStatusDistributionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetOrderStatusAnalyticsQueryHandler> _logger;

        public GetOrderStatusAnalyticsQueryHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<GetOrderStatusAnalyticsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<OrderStatusDistributionDto>> Handle(GetOrderStatusAnalyticsQuery request, CancellationToken cancellationToken)
        {

            if (request.StartDate.HasValue && request.StartDate.Value != default &&
                request.EndDate.HasValue && request.EndDate.Value != default &&
                request.StartDate.Value > request.EndDate.Value)
            {
                _logger.LogWarning("Invalid date range: StartDate {StartDate} is after EndDate {EndDate}",
                    request.StartDate, request.EndDate);
                return _responseHandler.BadRequest<OrderStatusDistributionDto>(
                    "Start date cannot be after end date.");
            }

            var data = await _unitOfWork.Sales.GetOrderStatusDistributionAsync(
                request.BranchId,
                request.StartDate,
                request.EndDate);

            if (data == null || !data.Any())
            {
                _logger.LogWarning(
                    "No order status data found for BranchId: {BranchId}", request.BranchId);
                return _responseHandler.NotFound<OrderStatusDistributionDto>(
                    "No order data found for the given filters.");
            }

            var result = new OrderStatusDistributionDto
            {
                Statuses = data
            };


            return _responseHandler.Success(result);


        }
    }
}