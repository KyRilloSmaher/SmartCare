using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Enums
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Completed,
        Cancelled,
        Returned,
        PaymentFailed,
    }
}
