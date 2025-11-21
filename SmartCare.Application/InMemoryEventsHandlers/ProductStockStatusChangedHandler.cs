
using SmartCare.Application.Messaging;
using SmartCare.Application.Notifications;
using SmartCare.Domain.Events;

namespace SmartCare.API.EventHandlers
{
    public class ProductStockStatusChangedHandler
    {
        private readonly INotificationSender _notificationSender;

        public ProductStockStatusChangedHandler(IEventBus eventBus, INotificationSender notificationSender)
        {
            // Subscribe to product stock changes
            eventBus.Subscribe<ProductStockStatusChangedEvent>(HandleAsync);
            _notificationSender = notificationSender;
        }

        private async Task HandleAsync(ProductStockStatusChangedEvent evt)
        {
            // Notify only clients subscribed to this product group
            await _notificationSender.SendProductStockStatusChangedAsync(evt.ProductId , new
            {
                productId = evt.ProductId,
                isAvailable = evt.isAvailable
            });
            Console.WriteLine($"[SignalR------] Product {evt.ProductId} stock updated 12333333 isAvailable: {evt.isAvailable}");
        }
    }
}
