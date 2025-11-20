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
            Console.WriteLine("[DI] ReservationExpiredEventHandler constructed!");

            eventBus.Subscribe<ReservationExpiredEvent>(async evt =>
            {
                Console.WriteLine("[Subscribe] Handler registered for ReservationExpiredEvent");
                await HandleAsync(evt);
            });
        }

        private async Task HandleAsync(ReservationExpiredEvent evt)
        {
            Console.WriteLine("########## Handler Fired ##########");

            try
            {
                await _hubContext.Clients.Group($"cart:{evt.CartId}")
                    .SendAsync("ReservationExpired", new
                    {
                        productId = evt.ProductId,
                        quantity = evt.Quantity,
                        message = evt.Message
                    });

                Console.WriteLine($"[SignalR] Reservation expired for product {evt.ProductId} – cart {evt.CartId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR ERROR] {ex.Message}");
            }
        }

    }

}
