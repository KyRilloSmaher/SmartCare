using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;

namespace SmartCare.Domain.OrderStates
{
    /// <summary>
    /// Delivery has been accepted and is being prepared.
    /// </summary>
    public class DeliveryAcceptedState : IOrderState
    {
        public void Handle(Order order, OrderStatus nextStatus)
        {
            if (order is not OnlineOrder)
                throw new InvalidOperationException("Invalid order type");

            if (nextStatus == OrderStatus.Ready_To_Ship)
            {
                order.SetStatus(nextStatus);
                return;
            }

            throw new InvalidOperationException("Invalid delivery transition");
        }
    }
}