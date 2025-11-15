namespace SmartCare.Application.Events
{

    public class ReservationExpiredEvent
    {
        public Guid CartId { get; }
        public Guid ProductId { get; }
        public int Quantity { get; }
        public string Message { get; }

        public ReservationExpiredEvent(Guid cartId, Guid productId, int quantity, string message)
        {
            CartId = cartId;
            ProductId = productId;
            Quantity = quantity;
            Message = message;
        }
    }
}
