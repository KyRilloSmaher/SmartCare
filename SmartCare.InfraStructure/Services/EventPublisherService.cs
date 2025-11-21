using SmartCare.Application.commens;
using SmartCare.Application.Messaging;
using SmartCare.Application.IServices;

using SmartCare.Domain.Events;

namespace SmartCare.InfraStructure.Services
{
    public class EventPublisherService : IEventPublisherService
    {
        private readonly IEventBus _eventBus;

        public EventPublisherService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task PublishReservationExpired(Guid cartId, Guid productId, int quantity)
        {
            var evt = new ReservationExpiredEvent(cartId, productId, quantity,
                "Your reservation has expired and the item was removed from your cart.");
            await _eventBus.PublishAsync(evt);
        }

        public async Task PublishPaymentStatusChanged(Guid orderId, string clientId, string status, string message)
        {
            var evt = new PaymentStatusChangedEvent(orderId, clientId, status, message);
            await _eventBus.PublishAsync(evt);
        }

        public async Task PublishProductStockStatusChanged(Guid productId, bool isAvailable)
        {
            var evt = new ProductStockStatusChangedEvent(productId, isAvailable);
            await _eventBus.PublishAsync(evt);
        }

        public async Task PublishOrderExpirationNotification(Guid orderId)
        {
            var evt = new OrderExpiredEvent(orderId);
            await _eventBus.PublishAsync(evt);
        }
    }
}
