using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Responses
{
    public class OutOfStockItemDto
    {
        public Guid ProductId { get; set; }
        public int RequestedQty { get; set; }
        public int AvailableQty { get; set; }
    }

}
