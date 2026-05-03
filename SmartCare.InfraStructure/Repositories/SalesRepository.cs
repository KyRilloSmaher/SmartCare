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

        public SalesRepository(ApplicationDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // =========================================================================
        // PRIVATE HELPERS
        // =========================================================================

        /// <summary>
        /// Base query for completed, non-deleted orders scoped to a branch
        /// </summary>
        private IQueryable<Order> CompletedOrders(Guid? branchId)
        {
            IQueryable<Order> query = _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed);

            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = query.Where(o => o.Items.Any(oi => oi.Inventory.StoreId == branchId.Value));
            }

            return query;
        }

        /// <summary>
        /// Apply date range filter to Order query
        /// </summary>
        private IQueryable<Order> ApplyDateFilter(IQueryable<Order> query, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && startDate.Value != default)
                query = query.Where(o => o.CreatedAt >= startDate.Value);

            if (endDate.HasValue && endDate.Value != default)
                query = query.Where(o => o.CreatedAt < endDate.Value.Date.AddDays(1));

            return query;
        }

        // =========================================================================
        // 1. CATEGORY REVENUE
        // =========================================================================

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

        // =========================================================================
        // 2. COMPANY REVENUE
        // =========================================================================

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

            // Attribute each order's TotalPrice to every Company it contains.
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

        // =========================================================================
        // 3. BRANCH PERFORMANCE
        // =========================================================================

        public async Task<IEnumerable<BranchPerformance>> GetBranchPerformanceAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = from oi in _context.OrderItems
                        .AsNoTracking()
                        .Include(oi => oi.Order)
                        .Include(oi => oi.Inventory)
                            .ThenInclude(i => i.Store)
                        where !oi.Order.IsDeleted && oi.Order.Status == OrderStatus.Completed
                        where !startDate.HasValue || oi.Order.CreatedAt >= startDate.Value
                        where !endDate.HasValue || oi.Order.CreatedAt < endDate.Value.Date.AddDays(1)
                        select new
                        {
                            BranchId = oi.Inventory.Store.Id,
                            BranchName = oi.Inventory.Store.Name,
                            oi.OrderId,
                            oi.Order.TotalPrice,
                            oi.Order.OrderType
                        };

            var distinctOrders = await query
                .Distinct()
                .ToListAsync();

            var result = distinctOrders
                .GroupBy(x => new { x.BranchId, x.BranchName })
                .Select(g => new BranchPerformance
                {
                    BranchId = g.Key.BranchId,
                    BranchName = g.Key.BranchName,
                    Revenue = g.Sum(x => x.TotalPrice),
                    TotalOrders = g.Select(x => x.OrderId).Distinct().Count(),
                    OnlineOrders = g.Where(x => x.OrderType == OrderType.Online)
                                    .Select(x => x.OrderId).Distinct().Count(),
                    PickupOrders = g.Where(x => x.OrderType == OrderType.InStore)
                                    .Select(x => x.OrderId).Distinct().Count()
                })
                .OrderByDescending(b => b.Revenue)
                .ToList();

            return result;
        }

        // =========================================================================
        // 4. SALES CHANNEL ANALYTICS - FIXED (NO ToString() in query)
        // =========================================================================

        public async Task<IEnumerable<SalesChannelPerformance>> GetSalesChannelAnalyticsAsync(
            Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            // Get raw data first, then format in memory
            var rawResults = await query
                .GroupBy(o => o.OrderType)
                .Select(g => new
                {
                    Channel = g.Key,
                    OrdersCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(c => c.Channel)
                .ToListAsync();

            // Convert to DTO with string formatting in memory
            var results = rawResults.Select(r => new SalesChannelPerformance
            {
                Channel = r.Channel.ToString(),
                OrdersCount = r.OrdersCount,
                Revenue = r.Revenue
            });

            return results;
        }

        // =========================================================================
        // 5. REVENUE ANALYTICS (TIME-SERIES) - FIXED (NO string.Format in query)
        // =========================================================================

        public async Task<IEnumerable<RevenuePoint>> GetRevenueAnalyticsAsync(
            Guid? branchId, FilterIntervales interval, DateTime? startDate, DateTime? endDate)
        {
            var query = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            switch (interval)
            {
                case FilterIntervales.daily:
                    {
                        // Get raw data from database
                        var rawResults = await query
                            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month, o.CreatedAt.Day })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                g.Key.Day,
                                Revenue = g.Sum(o => o.TotalPrice)
                            })
                            .OrderBy(r => r.Year).ThenBy(r => r.Month).ThenBy(r => r.Day)
                            .ToListAsync();

                        // Format date in memory
                        var results = rawResults.Select(r => new RevenuePoint
                        {
                            Date = $"{r.Year:D4}-{r.Month:D2}-{r.Day:D2}",
                            Revenue = r.Revenue
                        });

                        return results;
                    }

                case FilterIntervales.weekly:
                    {
                        var rawResults = await query
                            .GroupBy(o => EF.Functions.DateDiffWeek(DateTime.MinValue, o.CreatedAt))
                            .Select(g => new
                            {
                                WeekNumber = g.Key,
                                Revenue = g.Sum(o => o.TotalPrice)
                            })
                            .OrderBy(r => r.WeekNumber)
                            .ToListAsync();

                        var results = rawResults.Select(r => new RevenuePoint
                        {
                            Date = $"Week-{r.WeekNumber}:D2",
                            Revenue = r.Revenue
                        });

                        return results;
                    }

                case FilterIntervales.Annually:
                    {
                        var rawResults = await query
                            .GroupBy(o => o.CreatedAt.Year)
                            .Select(g => new
                            {
                                Year = g.Key,
                                Revenue = g.Sum(o => o.TotalPrice)
                            })
                            .OrderBy(r => r.Year)
                            .ToListAsync();

                        var results = rawResults.Select(r => new RevenuePoint
                        {
                            Date = r.Year.ToString(),
                            Revenue = r.Revenue
                        });

                        return results;
                    }

                default: // monthly
                    {
                        var rawResults = await query
                            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                Revenue = g.Sum(o => o.TotalPrice)
                            })
                            .OrderBy(r => r.Year).ThenBy(r => r.Month)
                            .ToListAsync();

                        var results = rawResults.Select(r => new RevenuePoint
                        {
                            Date = $"{r.Year:D4}-{r.Month:D2}",
                            Revenue = r.Revenue
                        });

                        return results;
                    }
            }
        }

        // =========================================================================
        // 6. DASHBOARD SUMMARY - WORKING (No changes needed)
        // =========================================================================

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(
            Guid? branchId, DateTime? startDate, DateTime? endDate)
        {
            var ordersQuery = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            var totalRevenue = await ordersQuery.SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
            var totalOrders = await ordersQuery.CountAsync();
            var totalClients = await _context.Clients.Select(o => o.Id).Distinct().CountAsync();
            var totalBranches = await _context.Stores.CountAsync();
            var totalAids = await _context.Products.CountAsync();

            var avgOrderValue = totalOrders == 0 ? 0 : totalRevenue / totalOrders;

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

        // =========================================================================
        // 7. CLIENT ANALYTICS - WORKING
        // =========================================================================

        public async Task<ClientAnalyticsDto> GetClientAnalyticsAsync(
            Guid? branchId, FilterIntervales interval, DateTime? startDate, DateTime? endDate)
        {
            var ordersQuery = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            var clientsInPeriod = await ordersQuery
                .Select(o => o.ClientId)
                .Distinct()
                .ToListAsync();

            var allClientsWithOrders = await _context.Orders
                .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
                .Select(o => o.ClientId)
                .Distinct()
                .ToListAsync();

            var firstOrders = await _context.Orders
                .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
                .Where(o => clientsInPeriod.Contains(o.ClientId))
                .GroupBy(o => o.ClientId)
                .Select(g => new
                {
                    ClientId = g.Key,
                    FirstOrderDate = g.Min(x => x.CreatedAt)
                })
                .ToListAsync();

            var newClients = firstOrders.Count(f =>
                (!startDate.HasValue || f.FirstOrderDate >= startDate.Value) &&
                (!endDate.HasValue || f.FirstOrderDate < endDate.Value.Date.AddDays(1)));

            var activeClients = clientsInPeriod.Count;
            var returningClients = activeClients - newClients;
            var totalClientsEver = await _context.Clients.CountAsync();

            return new ClientAnalyticsDto
            {
                TotalClients = totalClientsEver,
                //ActiveClients = activeClients,
                NewClients = newClients,
                ReturningClients = returningClients,
                //RetentionRate = activeClients == 0 ? 0 : Math.Round((double)returningClients / activeClients * 100, 2)
            };
        }

        // =========================================================================
        // 8. ORDERS TREND - FIXED (NO string.Format in query)
        // =========================================================================

        public async Task<List<OrderTrendItemDto>> GetOrdersTrendAsync(
            Guid? branchId, FilterIntervales interval, DateTime? startDate, DateTime? endDate)
        {
            var query = ApplyDateFilter(CompletedOrders(branchId), startDate, endDate);

            switch (interval)
            {
                case FilterIntervales.daily:
                    {
                        var rawResults = await query
                            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month, o.CreatedAt.Day })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                g.Key.Day,
                                Orders = g.Count()
                            })
                            .OrderBy(r => r.Year).ThenBy(r => r.Month).ThenBy(r => r.Day)
                            .ToListAsync();

                        var results = rawResults.Select(r => new OrderTrendItemDto
                        {
                            Date = $"{r.Year:D4}-{r.Month:D2}-{r.Day:D2}",
                            Orders = r.Orders
                        }).ToList();

                        return results;
                    }

                case FilterIntervales.weekly:
                    {
                        var rawResults = await query
                            .GroupBy(o => EF.Functions.DateDiffWeek(DateTime.MinValue, o.CreatedAt))
                            .Select(g => new
                            {
                                WeekNumber = g.Key,
                                MinDate = g.Min(x => x.CreatedAt),
                                Orders = g.Count()
                            })
                            .OrderBy(r => r.WeekNumber)
                            .ToListAsync();

                        var results = rawResults.Select(r => new OrderTrendItemDto
                        {
                            Date = r.MinDate.ToString("yyyy-MM-dd"),
                            Orders = r.Orders
                        }).ToList();

                        return results;
                    }

                case FilterIntervales.monthly:
                    {
                        var rawResults = await query
                            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                Orders = g.Count()
                            })
                            .OrderBy(r => r.Year).ThenBy(r => r.Month)
                            .ToListAsync();

                        var results = rawResults.Select(r => new OrderTrendItemDto
                        {
                            Date = $"{r.Year:D4}-{r.Month:D2}",
                            Orders = r.Orders
                        }).ToList();

                        return results;
                    }

                case FilterIntervales.Annually:
                    {
                        var rawResults = await query
                            .GroupBy(o => o.CreatedAt.Year)
                            .Select(g => new
                            {
                                Year = g.Key,
                                Orders = g.Count()
                            })
                            .OrderBy(r => r.Year)
                            .ToListAsync();

                        var results = rawResults.Select(r => new OrderTrendItemDto
                        {
                            Date = r.Year.ToString(),
                            Orders = r.Orders
                        }).ToList();

                        return results;
                    }

                default:
                    return new List<OrderTrendItemDto>();
            }
        }

        // =========================================================================
        // 9. ORDER STATUS DISTRIBUTION - WORKING
        // =========================================================================

        public async Task<List<OrderStatusItemDto>> GetOrderStatusDistributionAsync(
            Guid? branchId, DateTime? startDate, DateTime? endDate)
        {
            IQueryable<Order> query = _context.Orders.AsNoTracking().Where(o => !o.IsDeleted);

            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = query.Where(o => o.Items.Any(oi => oi.Inventory.StoreId == branchId.Value));
            }

            query = ApplyDateFilter(query, startDate, endDate);

            var rawResults = await query
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(r => r.Count)
                .ToListAsync();

            var totalOrders = rawResults.Sum(r => r.Count);

            var results = rawResults.Select(r => new OrderStatusItemDto
            {
                Status = r.Status.ToString(),
                Count = r.Count,
                //Percentage = totalOrders == 0 ? 0 : Math.Round((double)r.Count / totalOrders * 100, 2)
            }).ToList();

            return results;
        }

        // =========================================================================
        // 10. CATEGORY CHANNELS
        // =========================================================================

        public async Task<CategoryChannelDto> GetCategoryChannelsAsync(
            Guid categoryId, Guid? branchId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var channelData = await _context.OrderItems
                .AsNoTracking()
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Product.CategoryId == categoryId)
                .Where(oi => !oi.Order.IsDeleted && oi.Order.Status == OrderStatus.Completed)
                .Where(oi => !branchId.HasValue || oi.Inventory.StoreId == branchId.Value)
                .Where(oi => !startDate.HasValue || oi.Order.CreatedAt >= startDate.Value)
                .Where(oi => !endDate.HasValue || oi.Order.CreatedAt < endDate.Value.Date.AddDays(1))
                .Select(oi => oi.Order.OrderType)
                .ToListAsync();

            if (!channelData.Any())
            {
                return new CategoryChannelDto
                {
                    Online = 0,
                    Offline = 0,
                    //TotalOrders = 0
                };
            }

            var totalOrders = channelData.Count;
            var onlineCount = channelData.Count(ot => ot == OrderType.Online);
            var offlineCount = channelData.Count(ot => ot == OrderType.InStore);

            return new CategoryChannelDto
            {
                Online = (int)Math.Round((double)onlineCount / totalOrders * 100),
                Offline = (int)Math.Round((double)offlineCount / totalOrders * 100),
                //TotalOrders = totalOrders
            };
        }
    }
}