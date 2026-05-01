using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;

namespace SmartCare.Domain.OrderStates
{
    /// <summary>
    /// Order is packed and ready to be shipped.
    /// </summary>
    public class ReadyToShipState : IOrderState
    {
        public void Handle(Order order, OrderStatus nextStatus)
        {
            if (nextStatus == OrderStatus.Shipped)
            {
                order.SetStatus(OrderStatus.Shipped);
                return;
            }

            throw new InvalidOperationException("Invalid Ready_To_Ship transition");
        }
    }
}