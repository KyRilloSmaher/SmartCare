using Microsoft.AspNetCore.SignalR;
using SmartCare.API.Hubs;
using SmartCare.Application.commons;
using SmartCare.Application.Events;

namespace SmartCare.API.InMemoryEventsHandlers
{
    public class PaymentStatusChangedHandler
    {
        private readonly IHubContext<PaymentsHub> _hubContext;

        public PaymentStatusChangedHandler(IEventBus eventBus, IHubContext<PaymentsHub> hubContext)
        {
            _hubContext = hubContext;

            // Subscribe to the event
            eventBus.Subscribe<PaymentStatusChangedEvent>(HandleAsync);
        }

        private async Task HandleAsync(PaymentStatusChangedEvent evt)
        {
            // Push notification to the right client group
            await _hubContext.Clients.Group($"user:{evt.ClientId}")
                .SendAsync("PaymentStatusChanged", new
                {
                    orderId = evt.OrderId,
                    status = evt.Status,
                    message = evt.Message
                });

            Console.WriteLine($"[SignalR] Sent PaymentStatusChanged for Order {evt.OrderId} to Client {evt.ClientId}");
        }
    }
}