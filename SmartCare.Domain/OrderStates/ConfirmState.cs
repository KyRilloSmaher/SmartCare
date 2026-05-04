using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;

namespace SmartCare.Domain.OrderStates
{
    /// <summary>
    /// Order confirmed after successful payment.
    /// Next step depends on order type (Pickup or Delivery).
    /// </summary>
    public class ConfirmedState : IOrderState
    {
        public void Handle(Order order, OrderStatus nextStatus)
        {
            // Pickup flow
            if (order is PickUpOrder &&
                nextStatus == OrderStatus.WaitingForPickup)
            {
                order.SetStatus(nextStatus);
                return;
            }

            // Delivery flow
            if (order is OnlineOrder &&
         nextStatus == OrderStatus.Ready_To_Ship)
            {
                order.SetStatus(nextStatus);
                return;
            }

            throw new InvalidOperationException("Invalid transition from Confirmed");
        }
    }
}