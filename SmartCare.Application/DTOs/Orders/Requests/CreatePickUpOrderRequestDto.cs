using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Requests
{
    public class CreatePickUpOrderRequestDto : CreateOrderRequestDto   
    {
        public Guid storeId { get; set; }
    }
}
