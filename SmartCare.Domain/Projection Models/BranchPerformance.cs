using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class BranchPerformance
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = default!;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

}
