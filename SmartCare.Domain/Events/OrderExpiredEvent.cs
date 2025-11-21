using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Events
{
    public class OrderExpiredEvent
    {
        public Guid OrderId { get; set; }
        public string Message { get; set; }

        public OrderExpiredEvent(Guid orderId)
        {
            OrderId = orderId;
            Message = "Your order has expired due to non-payment.";
        }
    }
}
