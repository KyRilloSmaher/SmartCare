using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Analytics.Categories;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Categories
{
    public class GetCategoryPerformanceHandler: IRequestHandler<GetCategoryPerformanceQuery, Response<List<CategoryPerformanceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetCategoryPerformanceHandler> _logger;

        public GetCategoryPerformanceHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<GetCategoryPerformanceHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<List<CategoryPerformanceDto>>> Handle(GetCategoryPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var sales = await _unitOfWork.Sales.GetCategoryRevenueAsync(request.BranchId, request.StartDate, request.EndDate);

                // Calculate total revenue
                var totalRevenue = sales.Sum(s => s.Revenue);

                // Map to DTO
                var listOfdtos = sales.Select(s => new CategoryPerformanceDto
                                {
                                    CategoryId = s.CategoryId,
                                    Category = s.CategoryName,
                                    Revenue = s.Revenue,
                                    Percentage = totalRevenue == 0 ? 0 : (int)Math.Round((s.Revenue / totalRevenue) * 100)
                                }).ToList();

                return _responseHandler.Success(listOfdtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get category performance for branch {BranchId}", request.BranchId);
                return _responseHandler.Failed<List<CategoryPerformanceDto>>("Failed to retrieve category performance.");
            }
        }
    }
}
