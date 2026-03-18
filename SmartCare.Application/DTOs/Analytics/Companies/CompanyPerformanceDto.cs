using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Analytics.Companies
{
    public class CompanyPerformanceDto
    {
        public Guid CompanyId { get; set; }
        public string Company { get; set; } = default!;
        public decimal Revenue { get; set; }
        public int Percentage { get; set; }
    }
}
