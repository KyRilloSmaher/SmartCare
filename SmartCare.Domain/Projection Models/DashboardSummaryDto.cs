using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalClients { get; set; }
        public decimal AvgOrderValue { get; set; }
        public int TotalBranches { get; set; }
        public int TotalAids { get; set; }
    }

}
