using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;

namespace SmartCare.Domain.OrderStates
{
    /// <summary>
    /// Order is currently out for delivery.
    /// </summary>
    public class ShippedState : IOrderState
    {
        public void Handle(Order order, OrderStatus nextStatus)
        {
            if (nextStatus == OrderStatus.Completed)
            {
                order.SetStatus(OrderStatus.Completed);
                return;
            }

            throw new InvalidOperationException("Invalid Shipped transition");
        }
    }
}