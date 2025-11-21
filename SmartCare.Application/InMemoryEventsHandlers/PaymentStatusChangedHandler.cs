
using SmartCare.Application.Messaging;
using SmartCare.Application.Notifications;
using SmartCare.Domain.Events;

namespace SmartCare.API.InMemoryEventsHandlers
{
    public class PaymentStatusChangedHandler
    {
        private readonly INotificationSender _notificationSender;
        public PaymentStatusChangedHandler(IEventBus eventBus, INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;

            // Subscribe to the event
            eventBus.Subscribe<PaymentStatusChangedEvent>(HandleAsync);
        }

        private async Task HandleAsync(PaymentStatusChangedEvent evt)
        {
            // Push notification to the right client group

            await _notificationSender.SendPaymentStatusChangedAsync(evt.ClientId, new
            {
                orderId = evt.OrderId,
                status = evt.Status,
                message = evt.Message
            });
            Console.WriteLine($"[SignalR] Sent PaymentStatusChanged for Order {evt.OrderId} to Client {evt.ClientId}");
        }
    }
}