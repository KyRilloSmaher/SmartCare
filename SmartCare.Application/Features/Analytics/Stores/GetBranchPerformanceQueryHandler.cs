using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Analytics.Stores;
using SmartCare.Application.DTOs.Analytics.Stores.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Analytics.Stores
{
    public class GetBranchPerformanceQueryHandler: IRequestHandler<GetBranchPerformanceQuery, Response<List<BranchPerformanceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetBranchPerformanceQueryHandler> _logger;

        public GetBranchPerformanceQueryHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<GetBranchPerformanceQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<List<BranchPerformanceDto>>> Handle(
            GetBranchPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var sales = await _unitOfWork.Sales
                    .GetBranchPerformanceAsync(request.StartDate, request.EndDate);

                // Total revenue for percentage calculation
                var totalRevenue = sales.Sum(s => s.Revenue);

                // Map to DTO
                var listOfDtos = sales.Select(s => new BranchPerformanceDto
                {
                    BranchId = s.BranchId,
                    BranchName = s.BranchName,
                    Revenue = s.Revenue,
                    TotalOrders = s.TotalOrders,
                    OnlineOrders = s.OnlineOrders,
                    PickupOrders = s.PickupOrders,
                    PercentageOfRevenue = totalRevenue == 0
                        ? 0
                        : (int)Math.Round((s.Revenue / totalRevenue) * 100)
                }).ToList();

                return _responseHandler.Success(listOfDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get Branch performance between {StartDate} and {EndDate}",
                    request.StartDate, request.EndDate);

                return _responseHandler.Failed<List<BranchPerformanceDto>>(
                    "Failed to retrieve Branch performance.");
            }
        }
    }
}