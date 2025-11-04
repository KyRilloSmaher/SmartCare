using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Requests
{
    public class CreateOrderRequestDto
    {
        public string clientId { get; set; }
        public Guid cartId { get; set; }
        public OrderType orderType { get; set; }
        public Guid? storeId { get; set; }
        public string? deliveryAddress { get; set; }
    }
}
