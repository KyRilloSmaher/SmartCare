using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Requests
{
    public class VerifyPickupOrderRequestDto
    {
        public Guid OrderId { get; set; }
        public string VerifyCode {  get; set; }
    }
}
