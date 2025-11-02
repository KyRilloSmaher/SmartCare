using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Payment
{
    public class PaymentSessionRequest
    {
        public decimal Amount { get; set; }
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
        public string OrderId { get; set; }
    }
}
