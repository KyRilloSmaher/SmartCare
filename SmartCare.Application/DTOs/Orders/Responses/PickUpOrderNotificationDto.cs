using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Responses
{
    public class PickUpOrderNotificationDto
    {
        public Guid OrderId { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string? PickupCode { get; set; }
        public List<PickUpOrderItemDto> Items { get; set; }
    }

    public class PickUpOrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}
