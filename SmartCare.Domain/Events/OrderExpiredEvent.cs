using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Events
{
    public class OrderExpiredEvent
    {
        public string UserId { get; set; }
        public Guid OrderId { get; set; }
        public string Message { get; set; }

        public OrderExpiredEvent(string UserId , Guid orderId)
        {
            this.UserId = UserId;
            OrderId = orderId;
            Message = "Your order has expired due to non-payment.";
        }
    }
}
