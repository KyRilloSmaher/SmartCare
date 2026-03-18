using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class RevenuePoint
    {
        public string Date { get; set; } = default!;
        public decimal Revenue { get; set; }
    }
}
