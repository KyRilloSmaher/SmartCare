using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Enums
{
    public enum ReservationStatus
    {
        ReservedUntilPayment,
        ReservedUntilCheckout,
        ReservedUntilPickup,
        Realesed,
        Extra,
        PaymentTimeOut,
        PaymentFailed,
        Completed,
        OrderTimeOut,
        PickUpExpired,
        OrderUpdated
    }
}
