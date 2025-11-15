    using Microsoft.AspNetCore.SignalR;
    using SmartCare.API.Hubs;
    using SmartCare.Application.commons;
    using SmartCare.Application.Events;

namespace SmartCare.API.InMemoryEventsHandlers
{

    public class ReservationExpiredEventHandler
    {
        private readonly IHubContext<CartHub> _hubContext;

        public ReservationExpiredEventHandler(IEventBus eventBus, IHubContext<CartHub> hubContext)
        {
            _hubContext = hubContext;

            // subscribe to the event
            eventBus.Subscribe<ReservationExpiredEvent>(HandleAsync);
        }

        private async Task HandleAsync(ReservationExpiredEvent evt)
        {
            await _hubContext.Clients.Group($"cart:{evt.CartId}")
                .SendAsync("ReservationExpired", new
                {
                    productId = evt.ProductId,
                    quantity = evt.Quantity,
                    message = evt.Message
                });

            Console.WriteLine(
                $"[SignalR] Reservation expired for product {evt.ProductId} – client {evt.CartId}"
            );
        }
    }

}
