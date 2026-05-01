using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;

namespace SmartCare.Domain.OrderStates
{
    /// <summary>
    /// Initial state of an order before payment confirmation.
    /// </summary>
    public class PendingState : IOrderState
    {
        public void Handle(Order order, OrderStatus nextStatus)
        {
            if ( order.OrderType == OrderType.InStore && nextStatus == OrderStatus.WaitingForPickup)
            {
                order.SetStatus(OrderStatus.WaitingForPickup);
                return;
            }
            else if ( order.OrderType == OrderType.Online && nextStatus == OrderStatus.Confirmed)
            {
                order.SetStatus(OrderStatus.Confirmed);
                return;
            }

            if (nextStatus == OrderStatus.PaymentFailed ||
                nextStatus == OrderStatus.Expired)
            {
                order.SetStatus(nextStatus);
                return;
            }

            throw new InvalidOperationException("Invalid transition from Pending");
        }
    }
}
