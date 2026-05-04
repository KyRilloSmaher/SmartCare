using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class GLobelProductStockLevel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ComanyName { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int TotalStockLevel { get; set; }
    }
}
