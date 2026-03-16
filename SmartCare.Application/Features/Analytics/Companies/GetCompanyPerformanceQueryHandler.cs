using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Analytics.Companies;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
namespace SmartCare.Application.Features.Analytics.Companies
{
    public class GetCompanyPerformanceQueryHandler : IRequestHandler<GetCompanyPerformanceQuery, Response<List<CompanyPerformanceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetCompanyPerformanceQueryHandler> _logger;

        public GetCompanyPerformanceQueryHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<GetCompanyPerformanceQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<List<CompanyPerformanceDto>>> Handle(GetCompanyPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var sales = await _unitOfWork.Sales.GetCompanyRevenueAsync(request.BranchId, request.StartDate, request.EndDate);

                // Calculate total revenue
                var totalRevenue = sales.Sum(s => s.Revenue);

                // Map to DTO
                var listOfdtos = sales.Select(s => new CompanyPerformanceDto
                {
                    CompanyId = s.CompanyId,
                    Company = s.CompanyName,
                    Revenue = s.Revenue,
                    Percentage = totalRevenue == 0 ? 0 : (int)Math.Round((s.Revenue / totalRevenue) * 100)
                }).ToList();

                return _responseHandler.Success(listOfdtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Company performance for branch {BranchId}", request.BranchId);
                return _responseHandler.Failed<List<CompanyPerformanceDto>>("Failed to retrieve Company performance.");
            }
        }
    }
}
