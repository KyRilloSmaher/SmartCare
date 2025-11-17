using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Responses
{
    public class OrderItemResponseDtoForPickup : OrderItemResponseDto
    {
        public bool IsReadyForPickup { get; set; } 
    }
}
