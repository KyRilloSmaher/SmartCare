using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Analytics;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.Application.Features.Analytics.Clients
{
    public class GetClientAnalyticsQueryHandler
        : IRequestHandler<GetClientAnalyticsQuery, Response<ClientAnalyticsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetClientAnalyticsQueryHandler> _logger;

        public GetClientAnalyticsQueryHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<GetClientAnalyticsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<ClientAnalyticsDto>> Handle(
            GetClientAnalyticsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _unitOfWork.Sales.GetClientAnalyticsAsync(
                    request.BranchId,
                    request.interval,
                    request.StartDate,
                    request.EndDate);

                return _responseHandler.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to retrieve client analytics for branch {BranchId}",
                    request.BranchId);

                return _responseHandler.Failed<ClientAnalyticsDto>(
                    "Failed to retrieve client analytics.");
            }
        }
    }
}