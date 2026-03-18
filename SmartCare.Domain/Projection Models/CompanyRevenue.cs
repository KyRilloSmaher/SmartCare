using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class CompanyRevenue
    {
        public Guid CompanyId { get; set; } = default!;
        public string CompanyName { get; set; } = default!;
        public decimal Revenue { get; set; }
    }

}
