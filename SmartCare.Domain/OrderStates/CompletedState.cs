using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;

namespace SmartCare.Domain.OrderStates
{
    /// <summary>
    /// Final state. No further transitions allowed.
    /// </summary>
    public class CompletedState : IOrderState
    {
        public void Handle(Order order, OrderStatus nextStatus)
        {
            throw new InvalidOperationException("Completed is a terminal state");
        }
    }
}