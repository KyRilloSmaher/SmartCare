using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface ISalesRepository
    {
        Task<IEnumerable<CategoryRevenue>> GetCategoryRevenueAsync(Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<CompanyRevenue>> GetCompanyRevenueAsync(Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<BranchPerformance>> GetBranchPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<SalesChannelPerformance>> GetSalesChannelAnalyticsAsync(Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<RevenuePoint>> GetRevenueAnalyticsAsync(Guid? branchId, string interval, DateTime? startDate, DateTime? endDate);
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid? branchId,DateTime? startDate,DateTime? endDate);
        Task<ClientAnalyticsDto> GetClientAnalyticsAsync(Guid? branchId,string interval,DateTime? startDate,DateTime? endDate);
    }
    public class ClientAnalyticsDto
    {
        public int TotalClients { get; set; }
        public int NewClients { get; set; }
        public int ReturningClients { get; set; }
    }
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalClients { get; set; }
        public decimal AvgOrderValue { get; set; }
        public int TotalBranches { get; set; }
        public int TotalAids { get; set; }
    }
    public class SalesChannelPerformance
    {
        public string Channel { get; set; } = default!;
        public int OrdersCount { get; set; }
        public decimal Revenue { get; set; }
    }
    public class CategoryRevenue
    {
        public Guid CategoryId { get; set; } = default!;
        public string CategoryName { get; set; } = default!;
        public decimal Revenue { get; set; }
    }

    public class CompanyRevenue
    {
        public Guid CompanyId { get; set; } = default!;
        public string CompanyName { get; set; } = default!;
        public decimal Revenue { get; set; }
    }
    public class BranchPerformance
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = default!;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
    public class RevenuePoint
    {
        public string Date { get; set; } = default!;
        public decimal Revenue { get; set; }
    }
}
