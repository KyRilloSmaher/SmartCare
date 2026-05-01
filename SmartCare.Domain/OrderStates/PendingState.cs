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
            if (nextStatus == OrderStatus.WaitingForPickup)
            {
                order.SetStatus(OrderStatus.WaitingForPickup);
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
