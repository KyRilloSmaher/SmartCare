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
}
