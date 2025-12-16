
using SmartCare.Application.Messaging;
using SmartCare.Application.Notifications;
using SmartCare.Domain.Events;

namespace SmartCare.API.InMemoryEventsHandlers
{

    public class ReservationExpiredEventHandler
    {
        private readonly INotificationSender _notificationSender;

        public ReservationExpiredEventHandler(IEventBus eventBus, INotificationSender notificationSender)
        {
            Console.WriteLine("[DI] ReservationExpiredEventHandler constructed!");

            eventBus.Subscribe<ReservationExpiredEvent>(async evt =>
            {
                Console.WriteLine("[Subscribe] Handler registered for ReservationExpiredEvent");
                await HandleAsync(evt);
            });
            _notificationSender = notificationSender;
        }

        private async Task HandleAsync(ReservationExpiredEvent evt)
        {
            Console.WriteLine("########## Handler Fired ##########");

            try
            {

                await _notificationSender.SendCartUpdatedAsync(evt.UserId,evt.CartId, new
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
