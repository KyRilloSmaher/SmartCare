using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class LowStockProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = default!;
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
    }
}
