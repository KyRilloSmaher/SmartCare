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
        public string ClientId { get; set; }
        public Guid CartId { get; set; }
        public OrderType OrderType { get; set; }
        public Guid? storeId { get; set; }
        public Guid? deliveryAddressId { get; set; }
    }
}
