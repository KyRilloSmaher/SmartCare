using Microsoft.AspNetCore.SignalR;
using SmartCare.Application.Messaging;
using SmartCare.Application.Notifications;
using SmartCare.Domain.Events;

namespace SmartCare.Application.InMemoryEventsHandlers
{
    public class OrderExpireAlertHandler
    {
        private readonly INotificationSender _notificationSender;

        public OrderExpireAlertHandler(IEventBus eventBus, INotificationSender notificationSender)
        {
            // Subscribe to the event
            eventBus.Subscribe<OrderExpiredEvent>(HandleAsync);
            _notificationSender = notificationSender;
        }

        private async Task HandleAsync(OrderExpiredEvent evt)
        {
            // Push notification to the right client group
            await _notificationSender.SendOrderExpiration(evt.UserId , evt.OrderId, new
            {
                orderId = evt.OrderId,
                message = evt.Message
            });
            Console.WriteLine($"[SignalR] Sent Expiration Alert for Order {evt.OrderId} ");
        }
    }
}
