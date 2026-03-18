using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface ISalesRepository
    {
        Task<List<OrderTrendItemDto>> GetOrdersTrendAsync(Guid? branchId,string interval,DateTime? startDate,DateTime? endDate);
        Task<List<OrderStatusItemDto>> GetOrderStatusDistributionAsync(Guid? branchId,DateTime? startDate,DateTime? endDate);
        Task<IEnumerable<CategoryRevenue>> GetCategoryRevenueAsync(Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<CompanyRevenue>> GetCompanyRevenueAsync(Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<BranchPerformance>> GetBranchPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<SalesChannelPerformance>> GetSalesChannelAnalyticsAsync(Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<RevenuePoint>> GetRevenueAnalyticsAsync(Guid? branchId, string interval, DateTime? startDate, DateTime? endDate);
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid? branchId,DateTime? startDate,DateTime? endDate);
        Task<ClientAnalyticsDto> GetClientAnalyticsAsync(Guid? branchId,string interval,DateTime? startDate,DateTime? endDate);
    }
    
}
