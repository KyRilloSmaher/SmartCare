using SmartCare.Application.DTOs.Orders.Responses;

namespace SmartCare.Application.Notifications
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly INotificationSender _notificationSender;

        public OrderNotificationService(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task NotifyNewOnlineOrderAsync(
            Guid storeId,
            OnlineOrderResponseDto order,
            CancellationToken ct = default)
        {
            await _notificationSender.SendNewOnlineOrderToStoreAsync(storeId, order, ct);
        }

        public async Task NotifyNewPickUpOrderAsync(Guid storeId,PickUpOrderNotificationDto order,CancellationToken ct = default)
        {
            await _notificationSender.SendNewPickUpOrderToStoreAsync(storeId, order, ct);
        }
    }
}