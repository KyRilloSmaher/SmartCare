using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;

namespace SmartCare.Domain.OrderStates
{
    /// <summary>
    /// Creates the correct state object based on current order status.
    /// </summary>
    public static class OrderStateFactory
    {
        public static IOrderState Create(Order order)
        {
            return order.Status switch
            {
                OrderStatus.Pending => new PendingState(),
                OrderStatus.Confirmed => new ConfirmedState(),
                OrderStatus.WaitingForPickup => new WaitingForPickupState(),
                OrderStatus.DELIVERY_ACCEPTED => new DeliveryAcceptedState(),
                OrderStatus.Ready_To_Ship => new ReadyToShipState(),
                OrderStatus.Shipped => new ShippedState(),
                OrderStatus.Completed => new CompletedState(),
                _ => throw new NotSupportedException($"State {order.Status} not supported")
            };
        }
    }
}