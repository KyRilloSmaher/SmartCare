using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Requests
{
    public class CreateOnlineOrderRequestDto : CreateOrderRequestDto
    {
        public Guid deliveryAddressId { get; set; }
    }
}
