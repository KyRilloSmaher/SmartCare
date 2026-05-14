using NetTopologySuite.Index.HPRtree;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;

namespace SmartCare.Domain.OrderStates
{
    /// <summary>
    /// Order is ready for customer pickup.
    /// </summary>
    public class WaitingForPickupState : IOrderState
    {
        public void Handle(Order order, OrderStatus nextStatus)
        {
            if (order is not PickUpOrder)
                throw new InvalidOperationException("Invalid order type");

            if (nextStatus == OrderStatus.Completed)
            {
                order.SetStatus(OrderStatus.Completed);
                return;
            }

            throw new InvalidOperationException("Invalid pickup transition");
        }
    }
}