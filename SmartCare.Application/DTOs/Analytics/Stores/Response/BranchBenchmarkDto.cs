using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Analytics.Stores.Response
{
    public class BranchPerformanceDto
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = default!;
        public decimal Revenue { get; set; }
        public int OnlineOrders { get; set; }
        public int TotalOrders { get; set; }
        public int PickupOrders { get; set; }
        public int PercentageOfRevenue { get; set; }
    }
}
