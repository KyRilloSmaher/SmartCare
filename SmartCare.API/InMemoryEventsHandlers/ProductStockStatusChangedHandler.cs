using Microsoft.AspNetCore.SignalR;
using SmartCare.Application.commons;
using SmartCare.API.Hubs;
using SmartCare.Application.Events;

namespace SmartCare.API.EventHandlers
{
    public class ProductStockStatusChangedHandler
    {
        private readonly IHubContext<ProductsHub> _hubContext;

        public ProductStockStatusChangedHandler(IEventBus eventBus, IHubContext<ProductsHub> hubContext)
        {
            _hubContext = hubContext;

            // Subscribe to product stock changes
            eventBus.Subscribe<ProductStockStatusChangedEvent>(HandleAsync);
        }

        private async Task HandleAsync(ProductStockStatusChangedEvent evt)
        {
            // Notify only clients subscribed to this product group
            await _hubContext.Clients.Group($"product:{evt.ProductId}")
                .SendAsync("ProductStockStatusChanged", new
                {
                    productId = evt.ProductId,
                    isAvailable = evt.isAvailable
                });

            Console.WriteLine($"[SignalR] Product {evt.ProductId} stock updated → isAvailable: {evt.isAvailable}");
        }
    }
}
