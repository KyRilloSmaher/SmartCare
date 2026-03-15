using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly ApplicationDBContext _context;
        public SalesRepository(ApplicationDBContext context) => _context = context;

        public async Task<IEnumerable<CategoryRevenue>> GetCategoryRevenueAsync(Guid? branchId = null,DateTime? startDate = null,DateTime? endDate = null)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Include(oi => oi.Order)
                .Where(oi => !oi.Order.IsDeleted &&
                             oi.Order.Status == Domain.Enums.OrderStatus.Completed);

            // Apply branch filter if provided
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = query.Where(oi => oi.Inventory.StoreId == branchId.Value);
            }

            // Apply date filters if provided
            if (startDate.HasValue)
            {
                query = query.Where(oi => oi.Order.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Add one day to endDate to include the entire end date
                var endDateInclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(oi => oi.Order.CreatedAt < endDateInclusive);
            }

            var result = await query
                .GroupBy(oi => new {
                    CategoryId = oi.Product.Category.Id,
                    CategoryName = oi.Product.Category.Name
                })
                .Select(g => new CategoryRevenue
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Revenue = g.Sum(x => x.SubTotal)
                })
                .ToListAsync();

            return result;
        }
    }

}
