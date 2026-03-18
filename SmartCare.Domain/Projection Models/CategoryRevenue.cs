using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class CategoryRevenue
    {
        public Guid CategoryId { get; set; } = default!;
        public string CategoryName { get; set; } = default!;
        public decimal Revenue { get; set; }
    }


}
