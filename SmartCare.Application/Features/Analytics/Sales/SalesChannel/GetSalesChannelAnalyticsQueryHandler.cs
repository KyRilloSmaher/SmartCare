using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Analytics.Sales;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Analytics.Sales.SalesChannel
{
    public class GetSalesChannelAnalyticsQueryHandler
        : IRequestHandler<GetSalesChannelAnalyticsQuery, Response<List<SalesChannelDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetSalesChannelAnalyticsQueryHandler> _logger;

        public GetSalesChannelAnalyticsQueryHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<GetSalesChannelAnalyticsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<List<SalesChannelDto>>> Handle(
            GetSalesChannelAnalyticsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var sales = await _unitOfWork.Sales.GetSalesChannelAnalyticsAsync(
                    request.BranchId,
                    request.StartDate,
                    request.EndDate);

                var totalRevenue = sales.Sum(s => s.Revenue);

                var listOfDtos = sales.Select(s => new SalesChannelDto
                {
                    Channel = s.Channel,
                    OrdersCount = s.OrdersCount,
                    Revenue = s.Revenue,
                    Percentage = totalRevenue == 0
                        ? 0
                        : (int)Math.Round((s.Revenue / totalRevenue) * 100)
                }).ToList();

                return _responseHandler.Success(listOfDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get Sales Channel analytics for branch {BranchId}",
                    request.BranchId);

                return _responseHandler.Failed<List<SalesChannelDto>>(
                    "Failed to retrieve sales channel analytics.");
            }
        }
    }
}