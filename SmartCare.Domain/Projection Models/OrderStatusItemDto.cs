using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class OrderStatusItemDto
    {
        public string Status { get; set; } = default!;
        public int Count { get; set; }
    }
}
