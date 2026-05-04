using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Analytics.Sales;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Sales.Revenue
{
    public class GetRevenueAnalyticsQueryHandler
       : IRequestHandler<GetRevenueAnalyticsQuery, Response<RevenueAnalyticsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetRevenueAnalyticsQueryHandler> _logger;

        public GetRevenueAnalyticsQueryHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<GetRevenueAnalyticsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<RevenueAnalyticsDto>> Handle(
            GetRevenueAnalyticsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var revenue = await _unitOfWork.Sales.GetRevenueAnalyticsAsync(
                    request.BranchId,
                    request.interval,
                    request.StartDate,
                    request.EndDate);

                var dto = new RevenueAnalyticsDto
                {
                    Interval = request.interval,
                    Data = revenue.Select(r => new RevenuePointDto
                    {
                        Date = r.Date,
                        Revenue = r.Revenue
                    }).ToList()
                };

                return _responseHandler.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to retrieve revenue analytics for branch {BranchId}",
                    request.BranchId);

                return _responseHandler.Failed<RevenueAnalyticsDto>(
                    "Failed to retrieve revenue analytics.");
            }
        }
    }
}
