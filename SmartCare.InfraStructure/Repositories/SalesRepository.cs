using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
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
            if (startDate.HasValue && startDate.Value != default)
            {
                query = query.Where(oi => oi.Order.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue && endDate.Value != default)
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
        public async Task<IEnumerable<CompanyRevenue>> GetCompanyRevenueAsync(Guid? branchId = null,DateTime? startDate = null,DateTime? endDate = null)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Company)
                .Include(oi => oi.Order)
                .Include(oi => oi.Inventory)
                .Where(oi => !oi.Order.IsDeleted &&
                             oi.Order.Status == Domain.Enums.OrderStatus.Completed);

            // Apply branch filter if provided
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = query.Where(oi => oi.Inventory.StoreId == branchId.Value);
            }

            // Apply date filters if provided
            if (startDate.HasValue && startDate.Value != default)
            {
                query = query.Where(oi => oi.Order.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue && endDate.Value != default)
            {
                // Add one day to endDate to include the entire end date
                var endDateInclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(oi => oi.Order.CreatedAt < endDateInclusive);
            }

            var result = await query
                .GroupBy(oi => new {
                    CompanyId = oi.Product.Company.Id,
                    CompanyName = oi.Product.Company.Name
                })
                .Select(g => new CompanyRevenue
                {
                    CompanyId = g.Key.CompanyId,
                    CompanyName = g.Key.CompanyName,
                    Revenue = g.Sum(x => x.SubTotal)
                })
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<BranchPerformance>> GetBranchPerformanceAsync(DateTime? startDate = null,DateTime? endDate = null)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Inventory)
                    .ThenInclude(i => i.Store)
                .Where(oi =>
                    !oi.Order.IsDeleted &&
                    oi.Order.Status == Domain.Enums.OrderStatus.Completed);

            if (startDate.HasValue && startDate.Value != default)
            {
                query = query.Where(oi => oi.Order.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue && endDate.Value != default)
            {
                var endDateInclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(oi => oi.Order.CreatedAt < endDateInclusive);
            }

            var result = await query
                .GroupBy(oi => new
                {
                    BranchId = oi.Inventory.Store.Id,
                    BranchName = oi.Inventory.Store.Name
                })
                .Select(g => new BranchPerformance
                {
                    BranchId = g.Key.BranchId,
                    BranchName = g.Key.BranchName,
                    Revenue = g.Sum(x => x.SubTotal),
                    Orders = g.Select(x => x.OrderId).Distinct().Count()
                })
                .ToListAsync();

            return result;
        }
        public async Task<IEnumerable<SalesChannelPerformance>> GetSalesChannelAnalyticsAsync( Guid? branchId = null,DateTime? startDate = null ,DateTime? endDate = null)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => !oi.Order.IsDeleted &&
                             oi.Order.Status == Domain.Enums.OrderStatus.Completed);

            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = query.Where(oi => oi.Inventory.StoreId == branchId.Value);
            }

            if (startDate.HasValue && startDate.Value != default)
            {
                query = query.Where(oi => oi.Order.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue && endDate.Value != default)
            {
                var endDateInclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(oi => oi.Order.CreatedAt < endDateInclusive);
            }

            var result = await query
                .GroupBy(oi => oi.Order.OrderType) // Online / Pickup
                .Select(g => new SalesChannelPerformance
                {
                    Channel = g.Key.ToString().ToLower(),
                    OrdersCount = g.Select(x => x.OrderId).Distinct().Count(),
                    Revenue = g.Sum(x => x.SubTotal)
                })
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<RevenuePoint>> GetRevenueAnalyticsAsync(
    Guid? branchId,
    string interval,
    DateTime? startDate,
    DateTime? endDate)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Inventory)
                .Where(oi => !oi.Order.IsDeleted &&
                             oi.Order.Status == Domain.Enums.OrderStatus.Completed);

            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = query.Where(oi => oi.Inventory.StoreId == branchId.Value);
            }

            if (startDate.HasValue && startDate.Value != default)
            {
                query = query.Where(oi => oi.Order.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue && endDate.Value != default)
            {
                var endDateInclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(oi => oi.Order.CreatedAt < endDateInclusive);
            }

            switch (interval.ToLower())
            {
                case "daily":
                    {
                        var raw = await query
                            .GroupBy(o => new
                            {
                                o.Order.CreatedAt.Year,
                                o.Order.CreatedAt.Month,
                                o.Order.CreatedAt.Day
                            })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                g.Key.Day,
                                Revenue = g.Sum(x => x.SubTotal)
                            })
                            .ToListAsync();

                        return raw
                            .Select(g => new RevenuePoint
                            {
                                Date = $"{g.Year}-{g.Month:D2}-{g.Day:D2}",
                                Revenue = g.Revenue
                            })
                            .OrderBy(r => r.Date);
                    }

                case "weekly":
                    {
                        var raw = await query
                            .GroupBy(o => EF.Functions.DateDiffWeek(DateTime.MinValue, o.Order.CreatedAt))
                            .Select(g => new
                            {
                                WeekNumber = g.Key,
                                Revenue = g.Sum(x => x.SubTotal)
                            })
                            .ToListAsync();

                        return raw
                            .Select(g => new RevenuePoint
                            {
                                Date = $"Week-{g.WeekNumber}",
                                Revenue = g.Revenue
                            })
                            .OrderBy(r => r.Date);
                    }

                default: // monthly
                    {
                        var raw = await query
                            .GroupBy(o => new
                            {
                                o.Order.CreatedAt.Year,
                                o.Order.CreatedAt.Month
                            })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                Revenue = g.Sum(x => x.SubTotal)
                            })
                            .ToListAsync();

                        return raw
                            .Select(g => new RevenuePoint
                            {
                                Date = $"{g.Year}-{g.Month:D2}",
                                Revenue = g.Revenue
                            })
                            .OrderBy(r => r.Date);
                    }
            }
        }
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid? branchId,DateTime? startDate, DateTime? endDate)
        {
            var ordersQuery = _context.Orders
                .Where(o => !o.IsDeleted &&
                            o.Status == Domain.Enums.OrderStatus.Completed);

            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                ordersQuery = ordersQuery
                                 .OfType<PickUpOrder>()
                                 .Where(o => o.StoreId == branchId.Value);
            }

            if (startDate.HasValue && startDate.Value != default)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue && endDate.Value != default)
            {
                var endDateInclusive = endDate.Value.Date.AddDays(1);
                ordersQuery = ordersQuery.Where(o => o.CreatedAt < endDateInclusive);
            }

            // Aggregate Orders
            var totalRevenue = await ordersQuery.SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
            var totalOrders = await ordersQuery.CountAsync();

            var avgOrderValue = totalOrders == 0
                ? 0
                : totalRevenue / totalOrders;

            // Clients (distinct users who made orders)
            var totalClients = await ordersQuery
                .Select(o => o.ClientId)
                .Distinct()
                .CountAsync();

            // Global counts (not filtered)
            var totalBranches = await _context.Stores.CountAsync();
            var totalAids = await _context.Products.CountAsync(); // or your Aids table

            return new DashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalClients = totalClients,
                AvgOrderValue = Math.Round(avgOrderValue, 2),
                TotalBranches = totalBranches,
                TotalAids = totalAids
            };
        }

        public async Task<ClientAnalyticsDto> GetClientAnalyticsAsync(Guid? branchId,string interval,DateTime? startDate,DateTime? endDate)
        {
            var ordersQuery = _context.Orders
                .Where(o => !o.IsDeleted &&
                            o.Status == Domain.Enums.OrderStatus.Completed);

            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                ordersQuery = ordersQuery
                                    .OfType<PickUpOrder>()
                                    .Where(o => o.StoreId == branchId.Value);
            }

            // Period filter
            if (startDate.HasValue && startDate.Value != default)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue && endDate.Value != default)
            {
                var endDateInclusive = endDate.Value.Date.AddDays(1);
                ordersQuery = ordersQuery.Where(o => o.CreatedAt < endDateInclusive);
            }

            // Clients in period
            var clientsInPeriod = await ordersQuery
                .Select(o => o.ClientId)
                .Distinct()
                .ToListAsync();

            var totalClients = clientsInPeriod.Count;

            // Get first order date per user (GLOBAL)
            var firstOrders = await _context.Orders
                .Where(o => !o.IsDeleted &&
                            o.Status == Domain.Enums.OrderStatus.Completed)
                .GroupBy(o => o.ClientId)
                .Select(g => new
                {
                    UserId = g.Key,
                    FirstOrderDate = g.Min(x => x.CreatedAt)
                })
                .ToListAsync();

            // New clients
            var newClients = firstOrders
                .Count(f =>
                    clientsInPeriod.Contains(f.UserId) &&
                    (!startDate.HasValue || f.FirstOrderDate >= startDate.Value) &&
                    (!endDate.HasValue || f.FirstOrderDate < endDate.Value.AddDays(1))
                );

            // Returning clients
            var returningClients = totalClients - newClients;

            return new ClientAnalyticsDto
            {
                TotalClients = totalClients,
                NewClients = newClients,
                ReturningClients = returningClients
            };
        }
    }

}
