namespace SmartCare.Application.Events
{
    public class PaymentStatusChangedEvent
    {
        public Guid OrderId { get; }
        public string ClientId { get; }
        public string Status { get; }
        public string Message { get; }

        public PaymentStatusChangedEvent(Guid orderId, string clientId, string status, string message)
        {
            OrderId = orderId;
            ClientId = clientId;
            Status = status;
            Message = message;
        }
    }
}