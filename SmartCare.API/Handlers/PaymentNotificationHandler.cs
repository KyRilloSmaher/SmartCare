using Microsoft.AspNetCore.SignalR;
using SmartCare.API.Hubs;
using SmartCare.Application.commons;
using SmartCare.Application.Events;

namespace SmartCare.Application.Handlers
{
    public class PaymentNotificationHandler
    {
        private readonly IHubContext<PaymentsHub> _hubContext;

        public PaymentNotificationHandler(IHubContext<PaymentsHub> hubContext, IEventBus bus)
        {
            _hubContext = hubContext;
            bus.Subscribe<PaymentStatusChangedEvent>(HandleAsync);
        }

        private async Task HandleAsync(PaymentStatusChangedEvent evt)
        {
            await _hubContext.Clients.Group($"user:{evt.ClientId}")
                .SendAsync("PaymentStatusUpdated", new
                {
                    evt.OrderId,
                    evt.Status,
                    evt.Message
                });
        }
    }
}