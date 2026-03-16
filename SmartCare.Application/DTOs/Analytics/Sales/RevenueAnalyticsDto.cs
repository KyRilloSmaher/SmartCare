using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Analytics.Sales
{
    public class RevenuePointDto
    {
        public string Date { get; set; } = default!;
        public decimal Revenue { get; set; }
    }

    public class RevenueAnalyticsDto
    {
        public string Interval { get; set; } = default!;
        public List<RevenuePointDto> Data { get; set; } = new();
    }
}
