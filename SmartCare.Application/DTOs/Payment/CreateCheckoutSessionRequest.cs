using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Payment
{
    public class CreateCheckoutSessionRequest
    {
        public Guid OrderId { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
    }
}
