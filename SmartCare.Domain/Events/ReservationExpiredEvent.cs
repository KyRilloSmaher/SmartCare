namespace SmartCare.Domain.Events
{

    public class ReservationExpiredEvent
    {
        public Guid CartId { get; }
        public Guid ProductId { get; }
        public int Quantity { get; }
        public string Message { get; }
        public string UserId { get; }

        public ReservationExpiredEvent(Guid cartId, Guid productId, int quantity, string message, string userId)
        {
            CartId = cartId;
            ProductId = productId;
            Quantity = quantity;
            Message = message;
            UserId = userId;
        }
    }
}
