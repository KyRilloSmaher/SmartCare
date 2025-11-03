using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Responses
{
    public class OrderItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid InvetoryId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public ProductResponseForOrderDTo product { get; set; }
    }
}
