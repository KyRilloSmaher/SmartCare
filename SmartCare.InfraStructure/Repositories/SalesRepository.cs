using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly ApplicationDBContext _context;
        public SalesRepository(ApplicationDBContext context) => _context = context;

        // ---------------------------------------------------------------------------
        // Helper: base query for completed, non-deleted orders scoped to a branch.
        // When branchId is provided we go through OrderItems → Inventory so we can
        // filter by store, then project back to the distinct Orders.
        // ---------------------------------------------------------------------------
        private IQueryable<Order> CompletedOrders(Guid? branchId)
        {
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                return _context.OrderItems
                    .Where(oi => oi.Inventory.StoreId == branchId.Value)
                    .Select(oi => oi.Order)
                    .Distinct()
                    .Where(o => !o.IsDeleted && o.Status == (OrderStatus)5);
            }

            return _context.Orders
                .Where(o => !o.IsDeleted && o.Status == (OrderStatus)5);
        }

        // ---------------------------------------------------------------------------
        // Helper: apply optional date range to an Order query.
        // ---------------------------------------------------------------------------
        private static IQueryable<Order> ApplyDateFilter(
            IQueryable<Order> query, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && startDate.Value != default)
                query = query.Where(o => o.CreatedAt >= startDate.Value);

            if (endDate.HasValue && endDate.Value != default)
                query = query.Where(o => o.CreatedAt < endDate.Value.Date.AddDays(1));

            return query;
        }

        // ---------------------------------------------------------------------------
        // Category Revenue
        // Revenue = sum of Order.TotalPrice for orders that contain items in that category.
        // ---------------------------------------------------------------------------
        public async Task<IEnumerable<CategoryRevenue>> GetCategoryRevenueAsync(
            Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var ordersQuery = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);
            var orderPrices = await ordersQuery
                .Select(o => new { o.Id, o.TotalPrice })
                .ToListAsync();

            var orderIds = orderPrices.Select(o => o.Id).ToHashSet();
            var priceMap = orderPrices.ToDictionary(o => o.Id, o => o.TotalPrice);

            var items = await _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Where(oi => orderIds.Contains(oi.OrderId))
                .Select(oi => new
                {
                    oi.OrderId,
                    CategoryId = oi.Product.Category.Id,
                    CategoryName = oi.Product.Category.Name
                })
                .Distinct()
                .ToListAsync();

            // Attribute each order's TotalPrice to every category it contains.
            return items
                .GroupBy(x => new { x.CategoryId, x.CategoryName })
                .Select(g => new CategoryRevenue
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Revenue = g.Select(x => x.OrderId)
                                    .Distinct()
                                    .Sum(id => priceMap.GetValueOrDefault(id))
                })
                .ToList();
        }

        // ---------------------------------------------------------------------------
        // Company Revenue
        // ---------------------------------------------------------------------------
        public async Task<IEnumerable<CompanyRevenue>> GetCompanyRevenueAsync(
            Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var ordersQuery = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);
            var orderPrices = await ordersQuery
                .Select(o => new { o.Id, o.TotalPrice })
                .ToListAsync();

            var orderIds = orderPrices.Select(o => o.Id).ToHashSet();
            var priceMap = orderPrices.ToDictionary(o => o.Id, o => o.TotalPrice);

            var items = await _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Company)
                .Where(oi => orderIds.Contains(oi.OrderId))
                .Select(oi => new
                {
                    oi.OrderId,
                    CompanyId = oi.Product.Company.Id,
                    CompanyName = oi.Product.Company.Name
                })
                .Distinct()
                .ToListAsync();

            return items
                .GroupBy(x => new { x.CompanyId, x.CompanyName })
                .Select(g => new CompanyRevenue
                {
                    CompanyId = g.Key.CompanyId,
                    CompanyName = g.Key.CompanyName,
                    Revenue = g.Select(x => x.OrderId)
                                   .Distinct()
                                   .Sum(id => priceMap.GetValueOrDefault(id))
                })
                .ToList();
        }

        // ---------------------------------------------------------------------------
        // Branch Performance
        // ---------------------------------------------------------------------------
        public async Task<IEnumerable<BranchPerformance>> GetBranchPerformanceAsync( DateTime? startDate = null, DateTime? endDate = null)
        {

            var ordersPerBranch = _context.OrderItems
                .Where(oi => !oi.Order.IsDeleted && oi.Order.Status == (OrderStatus)5)
                .Where(oi => !startDate.HasValue || startDate.Value == default || oi.Order.CreatedAt >= startDate.Value)
                .Where(oi => !endDate.HasValue || endDate.Value == default || oi.Order.CreatedAt < endDate.Value.Date.AddDays(1))
                .Select(oi => new
                {
                    BranchId = oi.Inventory.Store.Id,
                    BranchName = oi.Inventory.Store.Name,
                    oi.OrderId,
                    oi.Order.TotalPrice,
                    oi.Order.OrderType
                })
                .Distinct(); 
            var result = await ordersPerBranch
                .GroupBy(x => new { x.BranchId, x.BranchName })
                .Select(g => new BranchPerformance
                {
                    BranchId = g.Key.BranchId,
                    BranchName = g.Key.BranchName,
                    Revenue = g.Sum(x => x.TotalPrice),

                    TotalOrders = g.Count(),

                    OnlineOrders = g.Count(x => x.OrderType == OrderType.Online),

                    PickupOrders = g.Count(x => x.OrderType == OrderType.InStore)
                })
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        // ---------------------------------------------------------------------------
        // Sales Channel Analytics
        // No item-level data needed — query Orders directly.
        // ---------------------------------------------------------------------------
        public async Task<IEnumerable<SalesChannelPerformance>> GetSalesChannelAnalyticsAsync(
            Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            return await query
                .GroupBy(o => o.OrderType)
                .Select(g => new SalesChannelPerformance
                {
                    Channel = g.Key.ToString().ToLower(),
                    OrdersCount = g.Count(),
                    Revenue = g.Sum(o => (decimal?)o.TotalPrice) ?? 0
                })
                .ToListAsync();
        }

        // ---------------------------------------------------------------------------
        // Revenue Analytics (time-series)
        // No item-level data needed — query Orders directly.
        // ---------------------------------------------------------------------------
        public async Task<IEnumerable<RevenuePoint>> GetRevenueAnalyticsAsync(
            Guid? branchId, FilterIntervales interval, DateTime? startDate, DateTime? endDate)
        {
            var query = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            switch (interval)
            {
                case FilterIntervales.daily:
                    {
                        var raw = await query
                            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month, o.CreatedAt.Day })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                g.Key.Day,
                                Revenue = g.Sum(o => (decimal?)o.TotalPrice) ?? 0
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

                case FilterIntervales.weekly:
                    {
                        var raw = await query
                            .GroupBy(o => EF.Functions.DateDiffWeek(DateTime.MinValue, o.CreatedAt))
                            .Select(g => new
                            {
                                WeekNumber = g.Key,
                                Revenue = g.Sum(o => (decimal?)o.TotalPrice) ?? 0
                            })
                            .ToListAsync();

                        return raw
                            .OrderBy(g => g.WeekNumber)
                            .Select(g => new RevenuePoint
                            {
                                Date = $"Week-{g.WeekNumber}",
                                Revenue = g.Revenue
                            });
                    }

                case FilterIntervales.Annually:
                    {
                        var raw = await query
                            .GroupBy(o => o.CreatedAt.Year)
                            .Select(g => new
                            {
                                Year = g.Key,
                                Revenue = g.Sum(o => (decimal?)o.TotalPrice) ?? 0
                            })
                            .ToListAsync();

                        return raw
                            .Select(g => new RevenuePoint
                            {
                                Date = $"{g.Year}",
                                Revenue = g.Revenue
                            })
                            .OrderBy(r => r.Date);
                    }

                default: // monthly
                    {
                        var raw = await query
                            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                Revenue = g.Sum(o => (decimal?)o.TotalPrice) ?? 0
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

        // ---------------------------------------------------------------------------
        // Dashboard Summary
        // ---------------------------------------------------------------------------
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(
            Guid? branchId, DateTime? startDate, DateTime? endDate)
        {
            var ordersQuery = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            var totalRevenue = await ordersQuery.SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
            var totalOrders = await ordersQuery.CountAsync();
            var avgOrderValue = totalOrders == 0 ? 0 : totalRevenue / totalOrders;

            var totalClients = await ordersQuery
                .Select(o => o.ClientId)
                .Distinct()
                .CountAsync();

            var totalBranches = await _context.Stores.CountAsync();
            var totalAids = await _context.Products.CountAsync();

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

        // ---------------------------------------------------------------------------
        // Client Analytics
        // ---------------------------------------------------------------------------
        public async Task<ClientAnalyticsDto> GetClientAnalyticsAsync(
            Guid? branchId, FilterIntervales interval, DateTime? startDate, DateTime? endDate)
        {
            var ordersQuery = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            var clientsInPeriod = await ordersQuery
                .Select(o => o.ClientId)
                .Distinct()
                .ToListAsync();

            var totalClients = clientsInPeriod.Count;

            // First-ever completed order date per client (global, not period-scoped).
            var firstOrders = await _context.Orders
                .Where(o => !o.IsDeleted && o.Status == (OrderStatus)5)
                .GroupBy(o => o.ClientId)
                .Select(g => new
                {
                    UserId = g.Key,
                    FirstOrderDate = g.Min(x => x.CreatedAt)
                })
                .ToListAsync();

            var newClients = firstOrders.Count(f =>
                clientsInPeriod.Contains(f.UserId) &&
                (!startDate.HasValue || f.FirstOrderDate >= startDate.Value) &&
                (!endDate.HasValue || f.FirstOrderDate < endDate.Value.AddDays(1)));

            return new ClientAnalyticsDto
            {
                TotalClients = totalClients,
                NewClients = newClients,
                ReturningClients = totalClients - newClients
            };
        }

        // ---------------------------------------------------------------------------
        // Orders Trend
        // ---------------------------------------------------------------------------
        public async Task<List<OrderTrendItemDto>> GetOrdersTrendAsync(
            Guid? branchId, FilterIntervales interval, DateTime? startDate, DateTime? endDate)
        {
            var query = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            switch (interval)
            {
                case FilterIntervales.weekly:
                    {
                        var raw = await query
                            .GroupBy(o => EF.Functions.DateDiffWeek(DateTime.MinValue, o.CreatedAt))
                            .Select(g => new
                            {
                                WeekNumber = g.Key,
                                MinDate = g.Min(x => x.CreatedAt),
                                Orders = g.Count()
                            })
                            .ToListAsync();

                        return raw
                            .OrderBy(g => g.WeekNumber)
                            .Select(g => new OrderTrendItemDto
                            {
                                Date = g.MinDate.ToString("yyyy-MM-dd"),
                                Orders = g.Orders
                            })
                            .ToList();
                    }

                case FilterIntervales.monthly:
                    {
                        var raw = await query
                            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                Orders = g.Count()
                            })
                            .ToListAsync();

                        return raw
                            .Select(g => new OrderTrendItemDto
                            {
                                Date = $"{g.Year}-{g.Month:D2}",
                                Orders = g.Orders
                            })
                            .OrderBy(r => r.Date)
                            .ToList();
                    }

                case FilterIntervales.Annually:
                    {
                        var raw = await query
                            .GroupBy(o => o.CreatedAt.Year)
                            .Select(g => new
                            {
                                Year = g.Key,
                                Orders = g.Count()
                            })
                            .ToListAsync();

                        return raw
                            .Select(g => new OrderTrendItemDto
                            {
                                Date = $"{g.Year}",
                                Orders = g.Orders
                            })
                            .OrderBy(r => r.Date)
                            .ToList();
                    }

                default: // daily
                    {
                        var raw = await query
                            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month, o.CreatedAt.Day })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                g.Key.Day,
                                Orders = g.Count()
                            })
                            .ToListAsync();

                        return raw
                            .Select(g => new OrderTrendItemDto
                            {
                                Date = $"{g.Year}-{g.Month:D2}-{g.Day:D2}",
                                Orders = g.Orders
                            })
                            .OrderBy(r => r.Date)
                            .ToList();
                    }
            }
        }

        // ---------------------------------------------------------------------------
        // Order Status Distribution
        // Not filtered to status=5 — all statuses are shown intentionally.
        // ---------------------------------------------------------------------------
        public async Task<List<OrderStatusItemDto>> GetOrderStatusDistributionAsync(
            Guid? branchId, DateTime? startDate, DateTime? endDate)
        {
            IQueryable<Order> query;

            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = _context.OrderItems
                    .Where(oi => oi.Inventory.StoreId == branchId.Value)
                    .Select(oi => oi.Order)
                    .Distinct()
                    .Where(o => !o.IsDeleted);
            }
            else
            {
                query = _context.Orders.Where(o => !o.IsDeleted);
            }

            query = ApplyDateFilter(query, startDate, endDate);

            var raw = await query
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return raw
                .Select(g => new OrderStatusItemDto
                {
                    Status = g.Status.ToString(),
                    Count = g.Count
                })
                .ToList();
        }
    }
}