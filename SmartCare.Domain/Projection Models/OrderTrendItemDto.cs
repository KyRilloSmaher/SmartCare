using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class OrdersTrendDto
    {
        public string Interval { get; set; } = default!;
        public List<OrderTrendItemDto> Data { get; set; } = new();
    }
    public class OrderTrendItemDto
    {
        public string Date { get; set; } = default!;
        public int Orders { get; set; }
    }
}
